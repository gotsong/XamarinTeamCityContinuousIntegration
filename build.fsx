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
            KeystorePassword = Environment.GetEnvironmentVariable("TodoKeystorePassword")
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

Target "ios-build" (fun () ->
    RestorePackages "TodoPCL.iOS.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "TodoPCL.iOS.sln"
            Configuration = "Debug"            
            Target = "Build"
            Platform = "iPhoneSimulator"
        })
)

Target "ios-adhoc" (fun () ->
    RestorePackages "TodoPCL.iOS.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "TodoPCL.iOS.sln"
            Configuration = "Ad-Hoc"
            Target = "Build"
            Platform = "iPhone"
            BuildIpa = true
        })

    let appPath = Directory.EnumerateFiles(Path.Combine("Todo.iOS", "bin", "iPhone", "Ad-Hoc"), "*.ipa", SearchOption.AllDirectories).First()

    TeamCityHelper.PublishArtifact appPath
)

Target "ios-appstore" (fun () ->
    RestorePackages "TodoPCL.iOS.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "TodoPCL.iOS.sln"
            Configuration = "AppStore"
            Target = "Build"
            Platform = "iPhone"
            BuildIpa = true
        })

    let outputFolder = Path.Combine("Todo.iOS", "bin", "iPhone", "AppStore")

    let appPath = Directory.EnumerateDirectories(outputFolder, "*.app").First()

    let zipFilePath = Path.Combine(outputFolder, "Todo.iOS.zip")

    let zipArgs = String.Format("-r -y '{0}' '{1}'", zipFilePath, appPath)

    Exec "zip" zipArgs

    TeamCityHelper.PublishArtifact zipFilePath

)

Target "ios-uitests" (fun () ->
   let appPath = Directory.EnumerateDirectories(Path.Combine("Todo.iOS", "bin", "iPhoneSimulator", "Debug"), "*.app").First()

   RunUITests appPath
)

"common-build"
  ==> "common-tests"

"android-build"
  ==> "android-package"
  ==> "android-uitests"
  
"ios-build"
  ==> "ios-uitests"

RunTargetOrDefault "common-tests"