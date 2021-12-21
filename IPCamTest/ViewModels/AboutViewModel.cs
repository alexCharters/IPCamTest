using MJPEG;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IPCamTest.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        byte[] frame;

        public byte[] Frame
        {
            get => frame;
            set => SetProperty(ref frame, value);
        }

        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));

            StreamDecoder decoder = new StreamDecoder();
            decoder.OnFrameReceived += (sender, e) =>
            {
                Debug.WriteLine("frame recieved");
                Debug.WriteLine(e);

                Frame = e.Frame;
            };
            decoder.StartDecodingAsync("http://192.168.200.234/axis-cgi/mjpg/video.cgi?camera=1");
            //decoder.StartDecodingAsync("http://127.0.0.1:8081/video.mjpg");
        }

        public ICommand OpenWebCommand { get; }
    }
}