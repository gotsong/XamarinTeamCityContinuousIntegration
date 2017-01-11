module BuildHelpers

open Fake
open Fake.XamarinHelper
open System
open System.IO
open System.Linq

let Exec command args =
    let result = Shell.Exec(command, args)

    if result <> 0 then failwithf "%s exited with error %d" command result

let RestorePackages solutionFile =
    Exec "nuget" ("restore " + solutionFile)
    solutionFile |> RestoreComponents (fun defaults -> {defaults with ToolPath = "tools/xpkg/xamarin-component.exe" })

let RunNUnitTests dllPath xmlPath =
    Exec "/Library/Frameworks/Mono.framework/Versions/Current/bin/nunit-console4" (dllPath + " -xml=" + xmlPath) 
    TeamCityHelper.sendTeamCityNUnitImport xmlPath

let RunUITests appPath =
    let testAppFolder = Path.Combine("TodoPCL.UITests", "testapps")

    if Directory.Exists(testAppFolder) then Directory.Delete(testAppFolder, true)
    Directory.CreateDirectory(testAppFolder) |> ignore

    let testAppPath = Path.Combine(testAppFolder, DirectoryInfo(appPath).Name)

    Directory.Move(appPath, testAppPath)

    RestorePackages "TodoPCL.UITests/TodoPCL.UITests.csproj"

    MSBuild "TodoPCL.UITests/bin/Release" "Build" [ ("Configuration", "Release"); ("Platform", "Any CPU") ] [ "TodoPCL.UITests/TodoPCL.UITests.csproj" ] |> ignore

    RunNUnitTests "TodoPCL.UITests/bin/Release/TodoPCL.UITests.dll" "TodoPCL.UITests/bin/Release/testresults.xml"


