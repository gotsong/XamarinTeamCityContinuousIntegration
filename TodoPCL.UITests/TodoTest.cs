using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Linq;
using System.IO;

namespace TodoPCL.UITests
{
	[TestFixture]
	public class TodoTests
	{
		private IApp _app;

		static readonly Func<AppQuery, AppQuery> TodoLabel = c => c.Marked("Todo");
		static readonly Func<AppQuery, AppQuery> AddButton = c => c.Marked ("+");

		static readonly Func<AppQuery, AppQuery> Name = c => c.Marked("Name");

		static readonly Func<AppQuery, AppQuery> EnterName = c => c.Marked("Name").Text("Test Name");
		static readonly Func<AppQuery, AppQuery> EnterNotes = c => c.Marked("MyLabel").Text("Test Notes");

		static readonly Func<AppQuery, AppQuery> SaveButton = c => c.Marked("Save");

		[SetUp]
		public void SetUp()
		{
			switch (TestEnvironment.Platform)
			{
			case TestPlatform.Local:
				var appFile = 
					new DirectoryInfo(Path.Combine("..","..", "testapps"))
						.GetFileSystemInfos()
						.OrderByDescending(file => file.LastWriteTimeUtc)
						.First(file => file.Name.EndsWith(".app") || file.Name.EndsWith(".apk"));

				_app = appFile.Name.EndsWith(".app")
					? ConfigureApp.iOS.AppBundle(appFile.FullName).StartApp() as IApp
					: ConfigureApp.Android.ApkFile(appFile.FullName).StartApp();
				break;
			case TestPlatform.TestCloudiOS:
				_app = ConfigureApp.iOS.StartApp();
				break;
			case TestPlatform.TestCloudAndroid:
				_app = ConfigureApp.Android.StartApp();
				break;
			}
		}

		[Test ()]
		public void TestCase ()
		{
			//_app.Repl ();


			AppResult[] result = _app.Query(TodoLabel);
			Assert.IsTrue(result.Any(), "Can't find 'Todo Label' on Home Screen!");
			_app.Screenshot("When I find the ToDo Label");

			_app.Tap(AddButton);
			result = _app.Query(Name);
			Assert.IsTrue(result.Any(), "Can't find 'Name Label' on Todo Entry Screen!");
			_app.Screenshot("When I find the Name Label");

		}
	}
}

