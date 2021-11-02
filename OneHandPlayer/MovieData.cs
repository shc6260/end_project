using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    class MovieData
    {
        private string _Title; // 파일 이름 저장 
        public string Title
        {
            get { return this._Title; }
            set { this._Title = value; }
        }

        private BitmapImage _ImageData; // 이미지 저장 멤버
        public BitmapImage ImageData
        {
            get { return this._ImageData; }
            set { this._ImageData = value; }
        }

        private String _Time;// 전체 시간 저장 메소드

        public String Time
        {

            get { return this._Time; }
            set { this._Time = value; }
        }

        private String _mediaSource;//영상 경로

        public String mediaSource
        {
            get { return this._mediaSource; }
            set { this._mediaSource = value; }
        }

        private bool _type;//폴더인지 영상인지 타입

        public bool type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        private int _star = 5;

        public int star
        {
            get { return this._star; }
            set { this._star = value; }
        }

        private String _starText = "";

        public String starText
        {
            get { return this._starText; }
            set { this._starText = value; }
        }
    }
}
