#r @"packages/FAKE/tools/FakeLib.dll"
#load "build-helpers.fsx"

open Fake
open System
open System.IO
open System.Linq
open BuildHelpers
open Fake.XamarinHelper

Target "common-build" (fun () ->
    RestorePackages "ToDoPCL.sln"

    MSBuild "Todo/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] [ "TodoPCL.sln" ] |> ignore
)


Target "common-tests" (fun () -> 
    RunNUnitTests "Todo/bin/Debug/TodoPCL.Tests.dll" "Todo/bin/Debug/testresults.xml" |> ignore  
)

Target "android-build" (fun () ->
    RestorePackages "TodoPCL.Android.sln"

    MSBuild "Todo.Android/bin/Release" "Build" [ ("Configuration", "Release") ] [ "TodoPCL.Android.sln" ] |> ignore

)

Target "android-package" (fun () ->
    AndroidPackage (fun defaults ->
        {defaults with
            ProjectPath = "Todo.Android/Todo.Android.csproj"
            Configuration = "Release"
            OutputPath = "Todo.Android/bin/Release"
        }) 
    |> AndroidSignAndAlign (fun defaults ->
        {defaults with 
            KeystorePath = "todo.keystore"
            KeystorePassword = Environment.GetEnvironmentVariable(“TodoKeystorePassword”)
            KeystoreAlias = "Todo"
        })
   |> fun file -> TeamCityHelper.PublishArtifact file.FullName
)

Target "android-uitests" (fun () ->
    AndroidPackage (fun defaults ->
        {defaults with
            ProjectPath = "Todo.Android/Todo.Android.csproj"
            Configuration = "Release"
            OutputPath = "Todo.Android/bin/Release"
        }) |> ignore

    let appPath = Directory.EnumerateFiles(Path.Combine("Todo.Android", "bin", "Release"), "*.apk", SearchOption.AllDirectories).First()

    RunUITests appPath
)



"common-build"
  ==> "common-tests"

"android-build"
  ==> "android-package"
  ==> "android-uitests"

RunTargetOrDefault "common-tests"