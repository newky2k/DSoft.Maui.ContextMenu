using System.Diagnostics;

namespace The49.Maui.ContextMenu.Sample.Pages;

public partial class ShowMenuOnClick : ContentPage
{
	public ShowMenuOnClick()
	{
		InitializeComponent();
	}

    private void Action_Clicked_1(object sender, EventArgs e)
    {
		Debug.WriteLine("DOH!");
    }
}