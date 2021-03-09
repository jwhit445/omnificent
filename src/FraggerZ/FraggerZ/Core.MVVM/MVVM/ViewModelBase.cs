using Prism.Navigation;

namespace Core.Omni.MVVM {
	public class ViewModelBase : BindableObjectBase, IDestructible {

		public ViewModelBase() {

		}

		///<summary>Called when the VM is being destroyed. Clean up any resources here.</summary>
		protected virtual void OnDestroy() {

		}

		public void Destroy() {
			OnDestroy();
		}

	}
}
