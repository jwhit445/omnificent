using System;
namespace Core.DependencyInjection {
	///<summary>Used for mocking time.</summary>
	public interface IClock {

		DateTime Now { get; }

		DateTime Today { get;  }
	}
}
