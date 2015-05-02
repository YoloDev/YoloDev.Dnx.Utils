using System;
using System.Collections.Generic;
using Xunit;

namespace YoloDev.Dnx.Utils
{
	public class PrecompilerTest
	{
		[Fact]
		public void NotNullParameterThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotNullMethod(null));
		}
		
		[Fact]
		public void NotEmptyStringParameterThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotEmptyStringMethod(null));
			Assert.Throws(typeof(ArgumentException), () => NotEmptyStringMethod(string.Empty));
			Assert.Throws(typeof(ArgumentException), () => NotEmptyStringMethod("  "));
		}
		
		[Fact]
		public void NotEmptyEnumerableParameterThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotEmptyEnumerableMethod(null));
			Assert.Throws(typeof(ArgumentException), () => NotEmptyEnumerableMethod(new string[0]));
		}
		
		[Fact]
		public void NotNullPropertyThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotNullProperty = null);
		}
		
		[Fact]
		public void NotEmptyStringPropertyThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotEmptyStringProperty = null);
			Assert.Throws(typeof(ArgumentException), () => NotEmptyStringProperty = string.Empty);
			Assert.Throws(typeof(ArgumentException), () => NotEmptyStringProperty = "  ");
		}
		
		[Fact]
		public void NotEmptyEnumerablePropertyThrows()
		{
			Assert.Throws(typeof(ArgumentNullException), () => NotEmptyEnumerableProperty = null);
			Assert.Throws(typeof(ArgumentException), () => NotEmptyEnumerableProperty = new string[0]);
		}
		
		private void NotNullMethod([NotNull] object input)
		{
		}
		
		private void NotEmptyStringMethod([NotEmpty] string input)
		{
		}
		
		private void NotEmptyEnumerableMethod([NotEmpty] IEnumerable<object> input)
		{
		}
		
		[NotNull]
		private object NotNullProperty { get; set; }
		
		[NotEmpty]
		private string NotEmptyStringProperty { get; set; }
		
		[NotEmpty]
		private IEnumerable<object> NotEmptyEnumerableProperty { get; set; }
	}
}