namespace Tests;

using System.IO;
using Components;
using NUnit.Framework;
using Services;
using UI;

public sealed class BaseUIControlTests {
	public class MockFocusable : IFocusable {
		public bool WasFocusedCalled = false;
		public void GrabFocus() => WasFocusedCalled = true;
	}

	[Test]
	public void TestHoverFocus() {
		var service = new HoverService();
		var mockControl = new MockFocusable();

		service.HandleHover(mockControl);

		if(!mockControl.WasFocusedCalled) {
			Assert.Fail("Hover Focus did not set focus on the control.");
		}
		else {
			Assert.Pass("Hover Focus correctly set focus on the control.");
		}
	}
}
