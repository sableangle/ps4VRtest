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
            任務一時間 > 任務二時間 筆數 （PS4）

         
            */
            ArrvTIme arrTime = new ArrvTIme();
            // VR 任務一 平均時間
            arrTime.VR_mission01 = rawdata.Where(m=>m.Mode == "VR").Sum(m=>m.mission01Time)/10;
            //  PS4 任務一 平均時間
            arrTime.PS4_mission01 = rawdata.Where(m=>m.Mode == "ps4").Sum(m=>m.mission01Time)/10;

            //VR 任務二 平均時間
            arrTime.VR_mission02 = rawdata.Where(m=>m.Mode == "VR").Sum(m=>m.mission02Time)/10;

            //PS4 任務二 平均時間
            arrTime.PS4_mission02 = rawdata.Where(m=>m.Mode == "ps4").Sum(m=>m.mission02Time)/10;
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


            return View();
        }

        public ActionResult Error()
        {
            return View();
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
        }
    }
}
