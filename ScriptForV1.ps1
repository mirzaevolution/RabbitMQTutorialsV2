$groups = @("C","C#","C++","Java","Python");
foreach($group in $groups)
{
    for($i = 0; $i -le 3; $i++)
    {
       dotnet run $group (New-Guid).Guid
    }
}