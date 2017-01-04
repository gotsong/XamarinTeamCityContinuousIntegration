using NUnit.Framework;
using System;
using Todo;

namespace TodoPCL.Tests
{
	[TestFixture ()]
	public class TodoItemTest
	{
		[Test ()]
		public void TodoItemDefaultsToNotDone ()
		{
			var todo = new TodoItem ();
			Assert.IsFalse(todo.Done);
		}
	}
}

