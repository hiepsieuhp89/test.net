using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Chat.Web.Helpers;
using Microsoft.AspNet.SignalR;
using Chat.Web.Hubs;
using Chat.Web.Models.ViewModels;
using Chat.Web.Models;
using System.Text.RegularExpressions;
using AutoMapper;
using System.Net;
using System.Text;
using System.Web.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Json;

namespace Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        static string sendGet(string url)
        {
            // Create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // GET request
            request.Method = "GET";
            request.ReadWriteTimeout = 5000;
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            // Return content
            string retString = myStreamReader.ReadToEnd();
            return retString;
        }
        public JsonResult GetIsSport()
        {
            string url = "http://api.isportsapi.com/sport/football/livescores/changes?api_key=wX9MOsuDjE0kk8Ix";

            // Call iSport Api to get data in json format
            string rsp = sendGet(url);
            ViewData["rsp"] = rsp;

            return Json(new { data = rsp }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            string url = "http://api.isportsapi.com/sport/football/livescores/changes?api_key=wX9MOsuDjE0kk8Ix";

            // Call iSport Api to get data in json format
            string rsp = sendGet(url);
            ViewData["rsp"] = rsp;

            Console.WriteLine("------------------------------------------" + rsp);

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var file = Request.Files[0];
                    string userReceiverId = "";
                    if (Request.Params.Count > 0)
                    {
                        if (Request.Params["userReceiverId"] != null)
                        {
                            userReceiverId = Request.Params["userReceiverId"];
                        }
                    }
                    // Some basic checks...
                    if (file != null && !FileValidator.ValidSize(file.ContentLength))
                        return Json("File size too big. Maximum File Size: 500KB");
                    else if (FileValidator.ValidType(file.ContentType))
                        return Json("This file extension is not allowed!");
                    else
                    {
                        // Save file to Disk
                        var fileName = DateTime.Now.ToString("yyyymmddMMss") + "_" + Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(Server.MapPath("~/Content/uploads/"), fileName);
                        file.SaveAs(filePath);

                        string htmlImage = string.Format(
                            "<a href=\"/Content/uploads/{0}\" target=\"_blank\">" +
                            "<img src=\"/Content/uploads/{0}\" class=\"post-image\">" +
                            "</a>", fileName);

                        using (var db = new ApplicationDbContext())
                        {
                            // Get sender & chat room
                            var senderViewModel = ChatHub._Connections.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                            var sender = db.Users.Where(u => u.UserName == senderViewModel.UserName).FirstOrDefault();
                            ApplicationUser receiver = null;
                            var room = new Room();
                            if (userReceiverId != null && userReceiverId != "")
                            {
                                receiver = db.Users.Where(u => u.Id == userReceiverId).FirstOrDefault();
                            }
                            else
                            {
                                room = db.Rooms.Where(r => r.Id == senderViewModel.CurrentRoomId).FirstOrDefault();
                            }


                            // Build message
                            Message msg = new Message();
                            if (receiver != null)
                            {
                                msg = new Message()
                                {
                                    Content = Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                                    Timestamp = DateTime.Now.Ticks.ToString(),
                                    FromUser = sender,
                                    ToUser = receiver,
                                };
                            }
                            else
                            {
                                msg = new Message()
                                {
                                    Content = Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                                    Timestamp = DateTime.Now.Ticks.ToString(),
                                    FromUser = sender,
                                    ToRoom = room,
                                };
                            }
                            
                            db.Messages.Add(msg);
                            db.SaveChanges();

                            // Send image-message to group
                            var messageViewModel = Mapper.Map<Message, MessageViewModel>(msg);
                            var hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                            hub.Clients.Group(senderViewModel.CurrentRoomId.ToString()).newMessage(messageViewModel);
                        }

                        return Json("Success");
                    }

                }
                catch (Exception ex)
                { return Json("Error while uploading" + ex.Message); }
            }

            return Json("No files selected");

        } // Upload

    }
}