using MJPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
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
            //decoder.StartDecodingAsync("http://192.168.200.234/axis-cgi/mjpg/video.cgi?camera=1");
            decoder.StartDecodingAsync("http://166.143.227.69:8000/-wvhttp-01-/GetOneShot?image_size=640x480&frame_count=1000000000");
            //decoder.StartDecodingAsync("http://127.0.0.1:8081/video.mjpg");

            MJPEGstart();
        }

        public ICommand OpenWebCommand { get; }

        private void MJPEGstart()
        {
            //Create an HTTP request, as long as the request does not end, MJPEG server will always send real-time image content to the response body of the request
            HttpWebRequest hwRequest = (System.Net.HttpWebRequest)WebRequest.Create("http://166.143.227.69:8000/-wvhttp-01-/GetOneShot?image_size=640x480&frame_count=1000000000");
            hwRequest.Method = "GET";
            HttpWebResponse hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            //Read the separator of each image specified by boundary, DroidCam is: - dcmjpeg
            string contentType = hwResponse.Headers["Content-Type"];
            string boundryKey = "boundary=";
            string boundary = contentType.Substring(contentType.IndexOf(boundryKey) + boundryKey.Length);

            //Get response volume flow
            Stream stream = hwResponse.GetResponseStream();
            List<char> yeetdata = new List<char>();

            while (stream.CanRead)
            {
                yeetdata.Add((char)stream.ReadByte());
            }

            byte[] bytes = new byte[1000];
            stream.Read(bytes, 0, 1000);

            //foreach(byte data in bytes)
            //{
            //    Debug.Write((char)data);
            //}

            string headerName = "Content-Length:";
            //Temporary storage of string data
            StringBuilder sb = new StringBuilder();
            int len = 1024;
            while (true)
            {
                //Read a line of data
                while (true)
                {
                    char c = (char)stream.ReadByte();
                    //Console.Write(c);
                    if (c == '\n')
                    {
                        break;
                    }
                    sb.Append(c);
                }
                string line = sb.ToString();
                sb.Remove(0, sb.Length);
                //Whether the current line contains content length:
                int i = line.IndexOf(headerName);
                if (i != -1)
                {
                    //Before each picture, there is a brief introduction to the picture (picture type and length). Here, we only care about the value after the length (content length:), which is used for subsequent reading of the picture
                    int imageFileLength = Convert.ToInt32(line.Substring(i + headerName.Length).Trim());
                    //Content-Length:xxx  After that, there will be a / r/n newline character, which will be the real image data
                    //Skip / r/n here
                    stream.Read(new byte[2], 0, 2);
                    //Start to read the image data. imageFileLength is the length after the content length: read
                    byte[] imageFileBytes = new byte[imageFileLength];
                    stream.Read(imageFileBytes, 0, imageFileBytes.Length);

                    foreach (byte data in imageFileBytes)
                    {
                        Debug.Write((char)data);
                    }

                    //JPEG The header of the file is: FF D8 FF ，The end of the file is: FF D9，very important，It's better to print when debugging, so as to distinguish whether the read-in data is exactly the same as all the contents of the picture
                    //Console.WriteLine("file header): + imagefilebytes [0]. ToString (" X ") +" + imagefilebytes [1]. ToString ("X") + "+ imagefilebytes [2]. ToString (" X ") +" + imagefilebytes [3]. ToString ("X") + "+ imagefilebytes [4]. ToString (" X "))));
                    //Console.WriteLine (end of file: + imagefilebytes [imagefilelength - 2]. ToString ("X") + "+ imagefilebytes [imagefilelength - 1]. ToString (" X ")));
                    //If the file read in is incomplete, the bigger the picture is, the faster the program cycle read speed is, and the more likely it is to lead to incomplete file read. If there is a good solution, I hope you can give me some advice. Thank you very much!
                    //Is the end of the file FF D9
                    if (imageFileBytes[imageFileLength - 2].ToString("X") != "FF" && imageFileBytes[imageFileLength - 1].ToString("X") != "D9")
                    {
                        //If the content of the read file is incomplete, skip the second file and let the stream position jump to the beginning of the next picture
                        //Console.WriteLine (start correction...);
                        char l = '0';
                        while (true)
                        {
                            char c = (char)stream.ReadByte();
                            //Here, only the first two characters in dcmjpeg are judged. When two consecutive characters in the read stream are, it means that the stream has read to the beginning of the next picture
                            if (l == boundary[0] && c == boundary[1])
                            {
                                break;
                            }
                            l = c;
                        }
                    }
                    else
                    {
                        //Read the picture successfully!
                        //accessImageHandler is an Action used to write pictures to PictureBox control in real time
                        Debug.WriteLine(imageFileBytes);
                    }
                    //If you sleep several tens of milliseconds properly here, it will reduce the situation of incomplete picture reading. The reason of incomplete picture random reading has not been found yet
                    Thread.Sleep(50);
                }
            }
            stream.Close();
            hwResponse.Close();
        }
    }
}