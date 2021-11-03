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
        private string memoRoute; //메모장 저장 경로
        StreamReader sr;//파일 읽기 객체
        StreamWriter sw;//파일 쓰기 객체

        public Memo()//클래스 생성시 메모장 루트 생성, 메모장이 없어도 클래스 생성시 자동 생성된다.
        {
            memoRoute = System.IO.Directory.GetCurrentDirectory() + @"\bookmark.txt";
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

            if (!find)
            {
                sw = new StreamWriter(memoRoute, true);
                sw.WriteLine(uritime);
                sw.Close();
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
                lines[count] = null;
                File.WriteAllLines(memoRoute, lines);
            }
        }

        //해당 영상의 북마크를 불러오는 함수
        public string[] bookMark_Output(Uri u)//매개변수로 현재 영상의 실행경로를 가져옴
        {
            sr = new StreamReader(memoRoute);
            string key;//메모장의 줄을 하나씩 읽어서 순차적으로 확인하는 용도
            char sp = '?';//실행경로와 북마크 시간을 나누어주는 용도
            string[] bookTime = new string[10];//북마크시간 저장할 스트링배열
            string uri = u.ToString();
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

    }
}
