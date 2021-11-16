using NReco.VideoInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
     class Image
    {
        static public BitmapImage LoadImage(string file, String filename, int time)//영상경로, 영상 이름, 영상 시간(초)
        {
            FileInfo fileInfo = new FileInfo("thumbnail/" + filename + ".jpeg");//스크린샷이 있으면 스크린샷 생성 안함
            if (fileInfo.Exists)
            {
                return new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "thumbnail/" + filename + ".jpeg"));
            }


            //MemoryStream stream = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(file, "thumbnail/" + filename + ".jpeg", time / 3);//동영상 경로, 출력 경로, 시간(초)


            return new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "thumbnail/" + filename + ".jpeg"));

        }

        static public BitmapImage LoadImage(string file,  int n, int time)//영상경로,  영상 시간(초)
        {
            FileInfo fileInfo = new FileInfo(file);
            String name = fileInfo.Name;

            fileInfo = new FileInfo("thumbnail/preview/" +name+ n + ".jpeg");//스크린샷이 있으면 스크린샷 생성 안함
            if (fileInfo.Exists)
            {
                return new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "thumbnail/preview/" + name + n + ".jpeg"));
            }
            //MemoryStream stream = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(file, "thumbnail/preview/" + name + n + ".jpeg", time);//동영상 경로, 출력 경로, 시간(초)


            return new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "thumbnail/preview/" + name + n + ".jpeg"));

        }

        static public int get_VedioTime(string file)//동영생 재생 시간 반환
        {
            FFProbe ffProbe = new FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(file);
            int time = (int)Math.Floor(videoInfo.Duration.TotalSeconds);

            return time;
        }


        static public String TimeToString(int time)
        {
            int hour = time / 3600;
            int minutes = time % 3600 / 60;
            int secends = time % 3600 % 60;



            return hour.ToString("00") + ":" + minutes.ToString("00") + ":" + secends.ToString("00");
        }
    }
}
