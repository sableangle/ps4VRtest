using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using TTest;
namespace PS4Test.Controllers
{
    public class HomeController : Controller
    {
        
        private IHostingEnvironment _he;
        public HomeController(IHostingEnvironment he)
        {
            _he = he;
        }  
       public ActionResult Index()
        {
            return View();
        }
        public ActionResult ShowData(){
            List<MissionData> rawdata = new List<MissionData>();
            string path = _he.WebRootPath + "/data/missionData.csv";
            var lines = System.IO.File.ReadAllLines(path);
            string line;
            
            //取得csv資料
            for (int i = 0; i < lines.Length; i++)
            {

                line = lines[i];

                string[] Word = line.Split(',');
                try{
                    rawdata.Add(new MissionData(convertUserNameToHidden(Word[0]),Word[1],double.Parse(Word[2]),double.Parse(Word[3]),Word[4]));
                }
                catch{
                    continue;
                }
            }


            return View(rawdata);
        }
        public string convertUserNameToHidden(string user){
            return user.FirstOrDefault() + "Ｏ" + user.LastOrDefault();
        }
        public ActionResult Mission()
        {

            List<MissionData> rawdata = new List<MissionData>();
            string path = _he.WebRootPath + "/data/missionData.csv";
            var lines = System.IO.File.ReadAllLines(path);
            string line;
            
            //取得csv資料
            for (int i = 0; i < lines.Length; i++)
            {

                line = lines[i];

                string[] Word = line.Split(',');
                try{
                    rawdata.Add(new MissionData(Word[0],Word[1],double.Parse(Word[2]),double.Parse(Word[3]),Word[4]));
                }
                catch{
                    continue;
                }
            }

            /*
            任務一時間 > 任務二時間 筆數 （VR）
            任務一時間 > 任務二時間 筆數 （PS4）*/
            //T 檢定實作 使用 VR 任務一時間 v.s 使用 PS4 任務一 
            List<double> mission01VR = new List<double>();
            List<double> mission01PS4 = new List<double>();

            List<double> mission02VR = new List<double>();
            List<double> mission02PS4 = new List<double>();

            mission01VR = rawdata.Where(m=>m.Mode == "VR").Select(m=>m.mission01Time).ToList();
            mission01PS4 = rawdata.Where(m=>m.Mode == "ps4").Select(m=>m.mission01Time).ToList();
            
            mission02VR = rawdata.Where(m=>m.Mode == "VR").Select(m=>m.mission02Time).ToList();
            mission02PS4 = rawdata.Where(m=>m.Mode == "ps4").Select(m=>m.mission02Time).ToList();


            var resultMission01 =  TTestController.TTest(mission01VR.ToArray(),mission01PS4.ToArray());
            var resultMission02 =  TTestController.TTest(mission02VR.ToArray(),mission02PS4.ToArray());

            ViewBag.TTestMission01 = resultMission01;
            ViewBag.TTestMission02 = resultMission02;
         
            
            ArrvTIme arrTime = new ArrvTIme();
            // VR 任務一 平均時間
            arrTime.VR_mission01 = rawdata.Where(m=>m.Mode == "VR").Sum(m=>m.mission01Time)/10;
            arrTime.Stand_VR_mission01 = StandardDeviation(mission01VR.ToArray());

            //  PS4 任務一 平均時間
            arrTime.PS4_mission01 = rawdata.Where(m=>m.Mode == "ps4").Sum(m=>m.mission01Time)/10;
            arrTime.Stand_PS4_mission01 = StandardDeviation(mission01PS4.ToArray());

            //VR 任務二 平均時間
            arrTime.VR_mission02 = rawdata.Where(m=>m.Mode == "VR").Sum(m=>m.mission02Time)/10;
            arrTime.Stand_VR_mission02 = StandardDeviation(mission02VR.ToArray());

            //PS4 任務二 平均時間
            arrTime.PS4_mission02 = rawdata.Where(m=>m.Mode == "ps4").Sum(m=>m.mission02Time)/10;        
            arrTime.Stand_PS4_mission02 = StandardDeviation(mission02PS4.ToArray());

            ViewBag.arrTime = arrTime;

            /* 任務二 - 死亡次數 （總額）
            任務二 - 死亡次數 （VR）
            任務二 - 死亡次數 （PS4）
            任務二 - 成功率（VR）
            任務二 - 成功率（PS4） */
            Mission02Detail mission02Detail = new Mission02Detail();
            mission02Detail.AllDieCount = rawdata.Where(m=>m.End == "死掉").Count();
            mission02Detail.VRDieCount = rawdata.Where(m=>m.End == "死掉" && m.Mode == "VR").Count();
            mission02Detail.PS4DieCount = rawdata.Where(m=>m.End == "死掉" && m.Mode == "ps4").Count();
            mission02Detail.PS4SuccessRate = (10-mission02Detail.PS4DieCount)/10;
            mission02Detail.VRSuccessRate = (float)(10-mission02Detail.VRDieCount)/10f;
            ViewBag.mission02Detail = mission02Detail;

            /*   
            同玩家進行任務一 PS4 比 VR 花費時間較短的比例
            同玩家進行任務二 PS4 比 VR 花費時間較短的比例
            */
            int mission01PS4betterThanVrTime = 0;
            int mission02PS4betterThanVrTime = 0;

            var groupByUser = rawdata.GroupBy(m=>m.User);
            foreach(var item in groupByUser){
                var a = item.Where(x=> x.Mode == "VR").FirstOrDefault().mission01Time;
                var b = item.Where(x=> x.Mode == "ps4").FirstOrDefault().mission01Time;
                var c = item.Where(x=> x.Mode == "VR").FirstOrDefault().mission02Time;
                var d = item.Where(x=> x.Mode == "ps4").FirstOrDefault().mission02Time;
                if(a > b){
                    mission01PS4betterThanVrTime ++;
                }
                if(c > d){
                    mission02PS4betterThanVrTime ++;
                }
            }
            PS4betterThanVRRate missionPs4BettenThanVR = new PS4betterThanVRRate();
            missionPs4BettenThanVR.mission01 = mission01PS4betterThanVrTime/10f;
            missionPs4BettenThanVR.mission02 = mission02PS4betterThanVrTime/10f;

            ViewBag.missionPs4BettenThanVR = missionPs4BettenThanVR;



            return View();
        }

        public ActionResult QUIS(){

            List<QuisData> quisrawdata = new List<QuisData>();
            string path = _he.WebRootPath + "/data/QUIS.csv";
            var lines = System.IO.File.ReadAllLines(path);
            string line;
            
            //取得csv資料
            for (int i = 0; i < lines.Length; i++)
            {

                line = lines[i];

                string[] Word = line.Split(',');
                try{
                    quisrawdata.Add(new QuisData(
                        Word[0],
                        Word[1],
                        Word[2],
                        Word[3],
                        Word[4],
                        Word[5],
                        int.Parse(Word[6]),
                        int.Parse(Word[7]),
                        int.Parse(Word[8]),
                        int.Parse(Word[9]),
                        int.Parse(Word[10]),
                        int.Parse(Word[11]),
                        int.Parse(Word[12]),
                        int.Parse(Word[13]),
                        int.Parse(Word[14]),
                        int.Parse(Word[15]),
                        int.Parse(Word[16]),
                        int.Parse(Word[17]),
                        int.Parse(Word[18]),
                        int.Parse(Word[19]),
                        int.Parse(Word[20]),
                        int.Parse(Word[21]),
                        int.Parse(Word[22]),
                        int.Parse(Word[23]),
                        int.Parse(Word[24]),
                        int.Parse(Word[25])
                        ));
                }
                catch(Exception ex){
                    continue;
                }
            }

            List<double> quisVR = new List<double>();
            List<double> quisPS4 = new List<double>();

            quisVR = quisrawdata.Where(m=>m.TestType == "PS VR + PS Move").Select(m=>m.Score_sum).ToList();
            quisPS4 = quisrawdata.Where(m=>m.TestType == "PS4 + 傳統搖桿").Select(m=>m.Score_sum).ToList();

            var quisTestResult =  TTestController.TTest(quisVR.ToArray(),quisPS4.ToArray());
            ViewBag.quisTestResult = quisTestResult;

            //計算 PS4 的 QUIS 總平均
            QuisArrvTime quisarrvTime = new QuisArrvTime();
            quisarrvTime.Ps4 = quisrawdata.Where(m=>m.TestType == "PS4 + 傳統搖桿").Sum(m=>m.Score_sum)/10f;
            quisarrvTime.VR = quisrawdata.Where(m=>m.TestType == "PS VR + PS Move").Sum(m=>m.Score_sum)/10f;
            quisarrvTime.Stand_VR = StandardDeviation(quisVR.ToArray());
            quisarrvTime.Stand_Ps4 = StandardDeviation(quisPS4.ToArray());

            ViewBag.quisarrvTime = quisarrvTime;



            var quisGroupByUser = quisrawdata.GroupBy(m=>m.User);
            Dictionary<string ,QuisArrvTime> persionQUIS = new  Dictionary<string ,QuisArrvTime>();
           
            foreach(var item in quisGroupByUser){
                QuisArrvTime temp = new QuisArrvTime();
                temp.Ps4 = item.Where(m=>m.TestType == "PS4 + 傳統搖桿").SingleOrDefault().Score_sum;
                temp.VR = item.Where(m=>m.TestType == "PS VR + PS Move").SingleOrDefault().Score_sum;
                persionQUIS.Add(convertUserNameToHidden(item.Key),temp);
            }
            ViewBag.persionQUIS = persionQUIS;


            return View();

        }


        public ActionResult Error()
        {
            return View();
        }
        public double Average(double[] num)
        {
            double sum = 0.0;

 

            foreach (double d in num)
            {
                sum += d;
            }

 

            return sum/Convert.ToDouble(num.Length);
        }
        public double StandardDeviation(double[] num)
        {
            double avg = Average(num);

            double SumOfSqrs = 0.0;

 

            foreach (double d in num)
            {
                SumOfSqrs += Math.Pow(d - avg, 2);
            }

 

            return Math.Sqrt((SumOfSqrs/(num.Length - 1)));
        }
        public class QuisData{
            public QuisData(
                string datetime,
                string user,
                string sex,
                string gameType,
                string gameDevice,
                string testType,
                int score_1_1,
                int score_1_2,
                int score_1_3,
                int score_1_4,
                int score_2_1,
                int score_2_2,
                int score_2_3,
                int score_3_1,
                int score_3_2,
                int score_3_3,
                int score_3_4,
                int score_4_1,
                int score_4_2,
                int score_4_3,
                int score_4_4,
                int score_5_1,
                int score_5_2,
                int score_5_3,
                int score_5_4,
                int score_sum
            ){
                Datetime = datetime;
                User = user;
                Sex = sex;
                GameType = gameType;
                GameDevice = gameDevice;
                TestType = testType;

                Score_1_1 = score_1_1;
                Score_1_2 = score_1_2;
                Score_1_3 = score_1_3;
                Score_1_4 = score_1_4;


                Score_2_1 = score_2_1;
                Score_2_2 = score_2_2;
                Score_2_3 = score_2_3;


                Score_3_1 = score_3_1;
                Score_3_2 = score_3_2;
                Score_3_3 = score_3_3;
                Score_3_4 = score_3_4;


                Score_4_1 = score_4_1;
                Score_4_2 = score_4_2;
                Score_4_3 = score_4_3;
                Score_4_4 = score_4_4;


                Score_5_1 = score_5_1;
                Score_5_2 = score_5_2;
                Score_5_3 = score_5_3;
                Score_5_4 = score_5_4;
                Score_sum = score_sum;
            }
            public string Datetime{get;set;}
            public string User{get;set;}
            public string Sex{get;set;}
            public string GameType{get;set;}
            public string GameDevice{get;set;}
            public string TestType{get;set;}
            public int Score_1_1{get;set;}
            public int Score_1_2{get;set;}
            public int Score_1_3{get;set;}
            public int Score_1_4{get;set;}
            public int Score_2_1{get;set;}
            public int Score_2_2{get;set;}
            public int Score_2_3{get;set;}
            public int Score_3_1{get;set;}
            public int Score_3_2{get;set;}
            public int Score_3_3{get;set;}
            public int Score_3_4{get;set;}
            public int Score_4_1{get;set;}
            public int Score_4_2{get;set;}
            public int Score_4_3{get;set;}
            public int Score_4_4{get;set;}
            public int Score_5_1{get;set;}
            public int Score_5_2{get;set;}
            public int Score_5_3{get;set;}
            public int Score_5_4{get;set;}
            public double Score_sum{set;get;}
        }
        public class MissionData{
            public double mission01Time{get;set;}
            public double mission02Time{get;set;}
            public string User{get;set;}
            public string Mode{get;set;}
            public string End{get;set;}
            public MissionData(string user,string mode,double time01,double time02,string end){
                mission01Time = time01;
                mission02Time = time02;
                User = user;
                Mode = mode;
                End = end;
            }

        }
        public class PS4betterThanVRRate{
            public float mission01{set;get;}
            public float mission02{set;get;}
        }
        public class Mission02Detail{
            public int AllDieCount{get;set;}
            public int VRDieCount{get;set;}
            public int PS4DieCount{get;set;}
            public float VRSuccessRate{get;set;}
            public float PS4SuccessRate{get;set;}

        }
        public class ArrvTIme{
            public double PS4_mission01{get;set;}
            public double PS4_mission02{get;set;}
            public double VR_mission01{get;set;}
            public double VR_mission02{get;set;}

            public double Stand_PS4_mission01{get;set;}
            public double Stand_PS4_mission02{get;set;}
            public double Stand_VR_mission01{get;set;}
            public double Stand_VR_mission02{get;set;}
        }

        public class QuisArrvTime{
            public double Ps4{get;set;}
            public double VR{set;get;}

            public double Stand_Ps4{get;set;}

            public double Stand_VR{get;set;}

        }
    }
}
