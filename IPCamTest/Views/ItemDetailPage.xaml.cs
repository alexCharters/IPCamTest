using IPCamTest.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace IPCamTest.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}