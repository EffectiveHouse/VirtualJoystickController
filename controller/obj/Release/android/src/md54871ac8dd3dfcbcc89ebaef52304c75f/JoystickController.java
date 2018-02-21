package md54871ac8dd3dfcbcc89ebaef52304c75f;


public class JoystickController
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("controller.JoystickController, controller, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", JoystickController.class, __md_methods);
	}


	public JoystickController ()
	{
		super ();
		if (getClass () == JoystickController.class)
			mono.android.TypeManager.Activate ("controller.JoystickController, controller, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
