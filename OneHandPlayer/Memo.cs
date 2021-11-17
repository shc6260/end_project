using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace WpfApp2
{
    class Memo
    {
        private string memoRoute = System.IO.Directory.GetCurrentDirectory() + @"\bookmark.txt"; //메모장 저장 경로
        StreamReader sr;//파일 읽기 객체
        StreamWriter sw;//파일 쓰기 객체
        private string jumpTime = "2";
        private string mouse_volume = "true";
        private string mouse_time = "true";
        private string mouse_click_pause = "true";
        private string mouse_click_fill = "true";
        private string mouse_wheel = "true";
        private string thumbnail = "true";
        public bool overlap;

        public Memo()//클래스 생성시 메모장 루트 생성, 메모장이 없어도 클래스 생성시 자동 생성된다.
        {
            if (!File.Exists(memoRoute))
            {
                using (File.Create(memoRoute)){ }
                sw = new StreamWriter(memoRoute, true);
                sw.WriteLine("jumptime*2");
                sw.WriteLine("mouse_volume>true");
                sw.WriteLine("mouse_time>true");
                sw.WriteLine("mouse_click_pause>true");
                sw.WriteLine("mouse_click_fill>true");
                sw.WriteLine("mouse_wheel>true");
                sw.WriteLine("thumbnail>true");
                sw.Close();
            }
            else
            {
                var lines = File.ReadAllLines(memoRoute);
                string[] check = lines[0].Split('*');
                jumpTime = check[1];
                check = lines[1].Split('>');
                mouse_volume = check[1];
                check = lines[2].Split('>');
                mouse_time = check[1];
                check = lines[3].Split('>');
                mouse_click_pause = check[1];
                check = lines[4].Split('>');
                mouse_click_fill = check[1];
                check = lines[5].Split('>');
                mouse_wheel = check[1];
                check = lines[6].Split('>');
                thumbnail = check[1];
            }
            
        }

        public string getjumpTime()
        {
            return jumpTime;
        }
        public string get_mouse_volume()
        {
            return mouse_volume;
        }
        public string get_mouse_time()
        {
            return mouse_time;
        }
        public string get_mouse_click_pause()
        {
            return mouse_click_pause;
        }
        public string get_mouse_click_fill()
        {
            return mouse_click_fill;
        }
        public string get_mouse_wheel()
        {
            return mouse_wheel;
        }
        public string get_thumbnail()
        {
            return thumbnail;
        }
        
        //점프 타임 수정
        public void JumpTimeSet(string time)
        {
            var lines = File.ReadAllLines(memoRoute);
            lines[0] = "jumptime*" + time;
            File.WriteAllLines(memoRoute, lines);
        }
        //세팅 수정
        public void settingSave(string mouse_volume, string mouse_time, string mouse_click_pause, string mouse_click_fill, string mouse_wheel, string thumbnail)
        {
            var lines = File.ReadAllLines(memoRoute);
            lines[1] = "mouse_volume>" + mouse_volume;
            lines[2] = "mouse_time>" + mouse_time;
            lines[3] = "mouse_click_pause>" + mouse_click_pause;
            lines[4] = "mouse_click_fill>" + mouse_click_fill;
            lines[5] = "mouse_wheel>" + mouse_wheel;
            lines[6] = "thumbnail>" + thumbnail;
            File.WriteAllLines(memoRoute, lines);
        }

        //메모장 안에 미디어파일경로와 북마크시간 넣는 함수
        public void bookMark_Inut(Uri u, string time)//매개변수로 현재 영상의 실행경로와 저장시간을 가져옴
        {
            //timem은 mediaMain.Position.ToString(@"mm\:ss") 형식으로 가져온다
            sr = new StreamReader(memoRoute);
            int count = 0;//해당영상의 북마크 저장갯수
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            string uri = u.ToString();
            bool find = false;

            string uritime = uri + "?" + time;

            while (sr.EndOfStream == false) //다음 줄이 없을때까지 한줄씩 불러와 읽음
            {
                key = sr.ReadLine();
                if (key.Contains(uritime)) //메모장에 해당 시간이 저장되 있는경우 저장하지않음
                {
                    find = true;
                    break;
                }
                else if (key.Contains(uri)) //메모장에 해당 영상에 대한 북마크가 10개이상 저장되 있는경우 저장하지않음
                {
                    count = count + 1;
                    if (count > 9)
                    {
                        find = true;
                        break;
                    }
                }
                find = false;
            }

            sr.Close();
            overlap = true;
            if (!find)
            {
                sw = new StreamWriter(memoRoute, true);
                sw.WriteLine(uritime);
                sw.Close();
                overlap = false;
            }
        }

        //해당 영상의 북마크 삭제하는 함수
        public void bookMark_Delete(Uri u, string time)//매개변수로 현재 영상의 실행경로와 저장시간을 가져옴
        {
            //timem은 mediaMain.Position.ToString(@"mm\:ss") 형식으로 가져온다
            sr = new StreamReader(memoRoute);
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            string uri = u.ToString();
            string uritime = uri + "?" + time;
            int count = -1;//특정구문이 들어가있는 줄번호
            bool find = false;

            while (sr.EndOfStream == false) //다음 줄이 없을때까지 한줄씩 불러와 읽음
            {
                key = sr.ReadLine();
                count = count + 1;
                if (key.Contains(uritime)) //메모장에 해당 내용을 찾으면 카운트 종료
                {
                    find = true;
                    break;
                }
            }
            sr.Close();
            if (find)
            {
                var lines = File.ReadAllLines(memoRoute);
                List<string> list = new List<string>();
                list = lines.ToList();
                list.RemoveAt(count);
                var line = list.ToArray();
                File.WriteAllLines(memoRoute, line);
            }
        }

        //해당 영상의 북마크를 불러오는 함수
        public string[] bookMark_Output(Uri u)//매개변수로 현재 영상의 실행경로를 가져옴
        {
            sr = new StreamReader(memoRoute);
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            string uri = u.ToString();
            char sp = '?';//실행경로와 북마크 시간을 나누어주는 용도
            string[] bookTime = new string[10];//북마크시간 저장할 스트링배열
            int count = 0;

            while (sr.EndOfStream == false) //다음 줄이 없을때까지 한줄씩 불러와 읽음
            {
                key = sr.ReadLine();
                if (key.Contains(uri + "?")) //메모장에 해당 영상경로의 북마크가 있으면 가져옴
                {
                    string[] check = key.Split(sp);
                    bookTime[count] = check[1];
                    count = count + 1;
                }

            }
            sr.Close();
            return bookTime;
        }

        //해당 영상의 별점 부여 함수
        public void starPoint_Input(string uri, string starText)
        {
            sr = new StreamReader(memoRoute);
            int count = -1;//특정구문이 들어가있는 줄번호
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            bool find = false;

            string uriStar = uri + "|" + starText;

            while (sr.EndOfStream == false) //다음 줄이 없을때까지 한줄씩 불러와 읽음
            {
                key = sr.ReadLine();
                count = count + 1;
                if (key.Contains(uri + "|")) //메모장에 해당 영상의 별점이 이미 저장되어있을때
                {
                    find = true;
                    break;
                }
            }

            sr.Close();

            if (find) //메모장에 해당 영상의 별점 이미 저장되어있을때 별점 수정
            {
                var lines = File.ReadAllLines(memoRoute);
                lines[count] = uriStar;
                File.WriteAllLines(memoRoute, lines);
            }
            else if (!find) //메모장에 해당 영상의 별점이 없을때 별점 부여
            {
                sw = new StreamWriter(memoRoute, true);
                sw.WriteLine(uriStar);
                sw.Close();
            }
        }


        //메모장에 저장되어있는 모든 별점을 긁어옴
        public int starPoint_Output(string uri)
        {
            sr = new StreamReader(memoRoute);
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            string[] starPoint = new string[2] { "0" , "0" };//영상 별점 저장할 문자열
            while (sr.EndOfStream == false) //다음 줄이 없을때까지 한줄씩 불러와 읽음
            {
                key = sr.ReadLine();
                if (key.Contains(uri+"|")) //메모장에 별점이 있는 영상은 전부 순차적으로 긁어옴
                {
                    starPoint = key.Split('|');
                }

            }
            sr.Close();
            return Convert.ToInt32(starPoint[1]);
        }

        //파일 이름 변경시 메모장 내용 변경
        public void Name_Change(string uri1, string uri2)//1 바꾸기전 이름 , 2 바꾼후 이름
        {
            var lines = File.ReadAllLines(memoRoute);
            int count = -1;//특정구문이 들어가있는 줄번호
            string[] book = new string[2] { "0", "0" };
            string[] star = new string[2] { "0", "0" };
            Uri uri = new Uri(uri1);
            string s_uri = uri.ToString();

            foreach(var line in lines)
            {
                count = count + 1;
                if (line.Contains(s_uri + "?")) //찾은값이 북마크일때
                {
                    book = line.Split('?');
                    lines[count] = new Uri(uri2).ToString() + "?" + book[1];
                    File.WriteAllLines(memoRoute, lines);
                }
                else if (line.Contains(uri1 + "|")) //찾은값이 별점일때
                {
                    star = line.Split('|');
                    lines[count] = uri2 + "|" + star[1];
                    File.WriteAllLines(memoRoute, lines);
                }
            }
        }

        //파일 삭제시 메모장 내용 변경
        public void data_Delete(string uri1)//매개변수로 현재 영상의 실행경로와 저장시간을 가져옴
        {
            var lines = File.ReadAllLines(memoRoute);
            Uri uri = new Uri(uri1);
            string uri2 = uri.ToString();
            int count = -1;//특정구문이 들어가있는 줄번호

            foreach (var line in lines)
            {
                count = count + 1;
                if (line.Contains(uri1)) //찾은값이 북마크일때
                {
                    lines[count] = null;
                    File.WriteAllLines(memoRoute, lines);
                }
                else if (line.Contains(uri2)) //찾은값이 별점일때
                {
                    lines[count] = null;
                    File.WriteAllLines(memoRoute, lines);
                }
            }
        }
    }
}
