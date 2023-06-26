using Discord_Chat__1_.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Discord_Chat__1_.Controllers
{
    public class HomeController : Controller
    {
        static int? to = null;
        SqlCommand com = new SqlCommand();
        SqlCommand selectDM = new SqlCommand();

        SqlConnection con = new SqlConnection();
        SqlDataReader dr;
        discordDBEntities db = new discordDBEntities();

        static userMessage userm = new userMessage();
        void connectionString()
        {
            con.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\New folder (2)\\Discord Chat (1)\\Discord Chat (1)\\App_Data\\discordDB.mdf\";Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework";
        }
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(userMessage r)
        {


            var registers = db.Users.ToList();

            foreach (var Item in registers)
            {
                if (Item.userEmail == r.currnetUser.userEmail && Item.userPassword == r.currnetUser.userPassword)
                {
                    Session["LoginUser"] = Item.userId;
                    userm.currnetUser = Item;
                    return RedirectToAction("PageUser");

                }

            }
            foreach (var item in registers)
            {
                if (item.userEmail != r.currnetUser.userEmail)
                {

                    ViewBag.userEmail = "error password or email";
                    return View("Login", item);

                }

            }


            return View("Error");


        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(User newregister)
        {
            db.Users.Add(newregister);
            db.SaveChanges();
            return View("Login");
        }
        public ActionResult PageUser(int? Id ,string messageBodyedit)
        {

            userm.listOfMessage.Clear();
            userm.userDM.Clear();
            connectionString();
            com.Connection = con;
            selectDM.Connection = con;
            selectDM.CommandText = "SELECT DISTINCT [messToUser] FROM [dbo].[messages] where [messFromUser] = " + userm.currnetUser.userId + " UNION SELECT DISTINCT [messFromUser] FROM [dbo].[messages] where [messtoUser] = " + userm.currnetUser.userId;
            con.Open();
            dr = selectDM.ExecuteReader();
            try
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var dbUser = db.Users.Find(Convert.ToInt32(dr["messToUser"]));
                        userm.userDM.Add(dbUser);
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            if (Id != null)
            {

                to = Id;
                com.CommandText = "SELECT * FROM [dbo].[messages] where messFromUser in (" + userm.currnetUser.userId + "," + to + ") and messToUser in (" + to + "," + userm.currnetUser.userId + ") order by messTime ASC";
                dr = com.ExecuteReader();
                try
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {

                            message dbmes = new message();
                            dbmes.messageId = Convert.ToInt32(dr["messageId"]);
                            dbmes.messFromUser = Convert.ToInt32(dr["messFromUser"]);
                            dbmes.messToUser = Convert.ToInt32(dr["messToUser"]);
                            dbmes.User = db.Users.Find(Convert.ToInt32(dr["messFromUser"]));
                            dbmes.User1 = db.Users.Find(Convert.ToInt32(dr["messToUser"]));
                            dbmes.messageBody = dr["messageBody"].ToString();
                            dbmes.messTime = Convert.ToDateTime(dr["messTime"]);
                            bool exist = userm.listOfMessage.Any(x => x.messageId == dbmes.messageId);
                            if (!exist)
                            {
                                userm.listOfMessage.Add(dbmes);
                            }

                        }

                    }
                }
                catch
                {
                    return View("Error");
                }

            }
            else
            {
                userm.listOfMessage.Clear();
            }
            //con.Close();
            return View(userm);


        }


        public ActionResult Forget()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Message(userMessage me, string messageBody)

        {

            if (to != null)
            {
                message mm = new message();
                mm.messFromUser = Convert.ToInt32(userm.currnetUser.userId);
                mm.messToUser = Convert.ToInt32(to);
                mm.messTime = Convert.ToDateTime(DateTime.Now);
                mm.messageBody = messageBody;
                db.messages.Add(mm);
                db.SaveChanges();
                return RedirectToAction("PageUser", new { Id = to });
            }
            else
            {
                return RedirectToAction("PageUser");
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(404);
            }
            message msg = db.messages.Find(id);
            if (msg == null)
            {

                return View("Error");
            }
            else
            {
                return View(msg);
            }
        }

        [HttpPost]
        public ActionResult Edit(message msg)
        {
            db.Entry(msg).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("PageUser", new { Id = to });
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(400);
            }
            message msg = db.messages.Find(id);
            if (msg == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.messages.Remove(msg);
                db.SaveChanges();
                return RedirectToAction("PageUser", new { Id = to });
            }
        }

        public ActionResult Settings()
        {
            return View(userm);
        }

        public ActionResult LogOut()
        {
            userm= null;
            return RedirectToAction("Login");
        }

        public ActionResult editInfoUser(int? id)
        {
            User  us = db.Users.Find(id);
            return View(us);
        }


        [HttpPost]
        public ActionResult editInfoUser(User us)
        {
            db.Entry(us).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Settings");
        }





        //[HttpPost]
        //public ActionResult Message()
        //{
        //    return View();
        //}
        //public PartialViewResult viewMessage()
        //{
        //    connectionString();
        //    com.Connection = con;
        //    com.CommandText = "SELECT [messageId]      ,[messFromUser]      ,[messToUser]      ,[messageBody]      ,[messTime]      ,[imagePath]  FROM [dbo].[messages] where messFromUser in (" + from + "," + to + ") and messToUser in (" + to + "," + from + ") order by messTime ASC";
        //    con.Open();
        //    return PartialView("_Message");
        //}
    }
}