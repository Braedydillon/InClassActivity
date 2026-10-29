param([string]$BaseUrl = 'http://localhost:5206', [string]$ApiUrl = 'http://localhost:5243')
$ErrorActionPreference='Stop'
$script:checks=0
function Check($condition,$message) { if (!$condition) { throw "FAILED: $message" }; $script:checks++; Write-Output "PASS: $message" }
function Page($path,$session) { Invoke-WebRequest "$BaseUrl$path" -WebSession $session -SkipHttpErrorCheck }
function Submit($path,$body,$session,$page) {
 $token=[regex]::Match($page.Content,'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
 $body['__RequestVerificationToken']=[System.Net.WebUtility]::HtmlDecode($token)
 Invoke-WebRequest "$BaseUrl$path" -Method Post -Body $body -WebSession $session -SkipHttpErrorCheck
}
$tag='mvctest-'+[guid]::NewGuid().ToString('N').Substring(0,12)
$teacherEmail="$tag-teacher@example.invalid"
$studentEmail="$tag-student@example.invalid"
$password='MvcTest!47-'+[guid]::NewGuid().ToString('N')
$teacher=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$student=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
Check ((Page '/' $student).Content.Contains('Your next chapter')) 'Landing page'
Check ((Invoke-WebRequest "$ApiUrl/swagger/index.html").StatusCode -eq 200) 'API Swagger'
Check ((Page '/Courses' $student).Content.Contains('Pick up where you left off.')) 'Anonymous users redirected to login'
$bad=Invoke-WebRequest "$BaseUrl/Account/Register" -Method Post -WebSession $student -Body @{} -SkipHttpErrorCheck
Check ($bad.StatusCode -eq 400) 'Antiforgery protection'
foreach($role in @('Teacher','Student')) {
 $session=if($role -eq 'Teacher'){$teacher}else{$student}
 $email=if($role -eq 'Teacher'){$teacherEmail}else{$studentEmail}
 $page=Page '/Account/Register' $session
 $body=@{Email=$email;Password=$password;ConfirmPassword=$password;FirstName='MVC';LastName=$role;Role=$role;StudentNumber="$tag-S";EmployeeNumber="$tag-T"}
 $result=Submit '/Account/Register' $body $session $page
 Check ($result.Content.Contains('Your account is ready.')) "$role registration"
 $result=Submit '/Account/Login' @{Email=$email;Password=$password} $session $result
 Check ($result.Content.Contains('THE COURSE COLLECTION')) "$role login through API"
}
Check ((Page '/Courses/Edit' $student).StatusCode -eq 403) 'Student cannot create a course'
$result=Submit '/Categories/Edit' @{Name=$tag;Description='MVC integration test'} $teacher (Page '/Categories/Edit' $teacher)
Check ($result.Content.Contains('Category saved.')) 'Teacher creates category'
$page=Page '/Courses/Edit' $teacher
$categoryId=[regex]::Match($page.Content,'<option value="(\d+)">'+$tag+'</option>').Groups[1].Value
Check (![string]::IsNullOrWhiteSpace($categoryId)) 'Course category dropdown loaded from API'
$result=Submit '/Courses/Edit' @{CourseId=0;Code=$tag;Title='MVC integration course';Description='End-to-end example';CourseCategoryId=$categoryId} $teacher $page
Check ($result.Content.Contains('Course saved.')) 'Teacher creates course'
$courseId=[regex]::Match($result.Content,'href="/Courses/Details/(\d+)"[^>]*>MVC integration course').Groups[1].Value
Check (![string]::IsNullOrWhiteSpace($courseId)) 'Created course listed in my teaching'
$details=Page "/Courses/Details/$courseId" $student
$result=Submit '/Courses/Enrol' @{id=$courseId} $student $details
Check ($result.Content.Contains('Enrolment requested.')) 'Student enrols'
$page=Page '/Learning' $student
Check ($page.Content.Contains('MVC integration course') -and $page.Content.Contains('Pending')) 'Student sees pending enrolment'
$page=Page "/Courses/Details/$courseId" $teacher
$enrolmentId=[regex]::Match($page.Content,'name="id" value="(\d+)"').Groups[1].Value
$result=Submit '/Courses/Status' @{id=$enrolmentId;courseId=$courseId;status='Confirmed'} $teacher $page
Check ($result.Content.Contains('Enrolment updated.') -and $result.Content.Contains('Save mark')) 'Teacher confirms enrolment'
$result=Submit '/Courses/Mark' @{enrolmentId=$enrolmentId;courseId=$courseId;resultId=0;finalMark='84.5'} $teacher $result
Check ($result.Content.Contains('Result saved.')) 'Teacher records decimal mark'
$page=Page '/Learning' $student
Check ($page.Content.Contains('84.5') -and $page.Content.Contains('Pass')) 'Student sees result'
$page=Page "/Courses/Details/$courseId" $teacher
$resultId=[regex]::Match($page.Content,'name="resultId" value="(\d+)"').Groups[1].Value
$result=Submit '/Courses/Mark' @{enrolmentId=$enrolmentId;courseId=$courseId;resultId=$resultId;finalMark='91.25'} $teacher $page
Check ($result.Content.Contains('Result saved.')) 'Teacher corrects result'
Check ((Page '/Learning' $student).Content.Contains('91.25')) 'Corrected result reaches student'
$result=Submit '/Courses/Delete' @{id=$courseId} $teacher $result
Check ($result.Content.Contains('Cannot delete a course with enrolments.')) 'Deletion conflict displayed'
$page=Page '/Account/Profile' $student
Check ($page.Content.Contains($studentEmail) -and !$page.Content.Contains($teacherEmail)) 'Independent user sessions'
$result=Submit '/Account/Profile' @{FirstName='Updated';LastName='Student'} $student $page
Check ($result.Content.Contains('Profile updated.') -and $result.Content.Contains('Updated')) 'Profile edit'
$result=Submit '/Account/Logout' @{} $student $result
Check ($result.Content.Contains('Create account')) 'Logout'
Check ((Page '/Learning' $student).Content.Contains('Pick up where you left off.')) 'Logout clears authentication'
Write-Output "All $script:checks checks passed. Test data prefix: $tag"
@{tag=$tag;teacherEmail=$teacherEmail;studentEmail=$studentEmail;courseId=$courseId;categoryId=$categoryId} | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'mvc-test-records.json')

