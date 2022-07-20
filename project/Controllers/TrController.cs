using System.Linq;
using System.Web.Mvc;

namespace project.Controllers
{
    public class TrController : Controller
    {
        // GET: Tr
        public System.Web.Mvc.ActionResult Index()
        {
            return View();
        }

        // Burayı register başarılı olduğu zaman home sayfasına view bag eklemek için oluşturduk.
        public System.Web.Mvc.ActionResult RegisterReturnIndex()
        {
            ViewBag.Message = "Registration Has Occurred Successfully";
            return View("Index");
        }

        // Register ulasmak için root linki /contorller adı(Tr) / action adı (Register)   
        public System.Web.Mvc.ActionResult Register()
        {
            return View();
        }

        // Giriş yapmış ise home sayfasına atıyor. 2. giriş yapmaya izin vermiyo.
        public System.Web.Mvc.ActionResult Login() // Get logini
        {
            User currentUser = (User)Session["CurrentUser"]; // currentuser obje . cast ederek user'e çeviriyoruz.
            if (currentUser != null)
            {
                return View("Index");
            }
            return View();
        }

        // kullanıcı giriş yapmamışsa çıkış yapamaz onu kontrol ediyoruz. 
        public System.Web.Mvc.ActionResult Logout()
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser != null)
            {
                Session.Remove("CurrentUser");
                Session.Abandon();
            }
            return View("Index");
        }

        // giriş yap kısmı
        
        [System.Web.Mvc.HttpPost]
        public System.Web.Mvc.ActionResult SignUp(User request)
        {
            using (calendarEntities dc = new calendarEntities())
            {
                var user = dc.Users.Where(x => x.Email == request.Email).FirstOrDefault();
                if (user != null)
                {
                    ViewBag.Message = "This Mail Is Used";
                    return View("Register");
                }
                else // yeni kayıt oluşturuyo bu kayıdıda session ekliyo. 
                {
                    dc.Users.Add(request);
                    dc.SaveChanges();
                    Session.Add("CurrentUser", request);
                    return RedirectToAction("RegisterReturnIndex");
                }
            }
        }

        // giriş bilgilerini databaseden çekilir. eğer kayıtlı user var ise section eklenir home sayfasına gönderir. yoksa hata mesajı ile kayıt sayfasına gönderir.
        [System.Web.Mvc.HttpPost]
        public System.Web.Mvc.ActionResult Login(User request) // giriş yapmanı sağlayan login
        {
            using (calendarEntities dc = new calendarEntities())
            {
                var user = dc.Users.Where(x => x.Email == request.Email && x.Password == request.Password).FirstOrDefault();

                if (user != null)
                {
                    Session.Add("CurrentUser", user);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = " The Information You Entered Is Missing Or Incorrect ";
                    return View("Login");
                }

            }
        }

        public System.Web.Mvc.ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public System.Web.Mvc.ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public System.Web.Mvc.ActionResult Calendar()
        {
            User currentUser = (User)Session["CurrentUser"]; // logini kontrol ediyor, eğer kullanıcı giriş yapmamış ise login sayfasına gönderiyor.
            if (currentUser != null)
            {
                return View();
            }
            return View("Login");
        }

        // javascript ile çağırılıyor.

        public JsonResult GetEvents()
        {
            User currentUser = (User)Session["CurrentUser"];
            using (calendarEntities dc = new calendarEntities())
            {
                // dc entity framework 
                // login yapmış user id ile o usere ait todolist ler calendara basılıyor. where linq ile gelen lise içerisinde filtre yapmayı sağlar.
                var events = dc.Events.Where(x => x.UserId == currentUser.UserId).ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }
        }

        public System.Web.Mvc.ActionResult Todolist()
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }

            using (calendarEntities dc = new calendarEntities())
            {
                // var değişkene atıyo variable tipi ne olursa olsun karşılıyor.
                var events = dc.Events.Where(x => x.UserId == currentUser.UserId).ToList();

                return View(events);
            }
        }

        // gelen event request te karşılanır. session da usere bakılır null ise logine atılır. user varsa data kaydedilir.
        [System.Web.Mvc.HttpPost] // post işlemi demek.
        public System.Web.Mvc.ActionResult CreateEvent(Events request)
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }

            using (calendarEntities dc = new calendarEntities())
            {
                request.UserId = currentUser.UserId;
                dc.Events.Add(request);
                dc.SaveChanges();
                return RedirectToAction("Todolist");
            }
        }

        // request te id karşılanır. kullanıcı giriş yapmamış ise logine gönderilir. 
        // giriş yapmış ise bu id le eşleşen kayıt bulunur ve silinir.
        public System.Web.Mvc.ActionResult DeleteEvent(int Id)
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }

            calendarEntities dc = new calendarEntities();
            var data = dc.Events.Where(x => x.EventID == Id).FirstOrDefault(); // firstordefult buda linq dur. bulursa ilk elemenı, bulamazsa null döner. id primery key oldugu için her zaman bitane döner.

            dc.Events.Remove(data);
            dc.SaveChanges();

            return RedirectToAction("Todolist");

        }
        // Bu kısım event update yeri
        // Genel request ve id karışılanır.
        [System.Web.Mvc.HttpPost]
        public System.Web.Mvc.ActionResult EditEvent(Events request, int Id)
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }

            // önce eşleşen data id ile çekilir. Sonra genel requestteki parametreler bu databaseden çekilmiş dataya update edilir.
            using (calendarEntities dc = new calendarEntities())
            {
                var result = dc.Events.Where(x => x.EventID == Id).FirstOrDefault();
                result.Description = request.Description;
                result.End = request.End;
                result.Start = request.Start;
                result.Subject = request.Subject;
                result.ThemeColor = request.ThemeColor;
                result.UserId = currentUser.UserId;
                dc.SaveChanges();
                return RedirectToAction("Todolist");
            }
        }

        // Update sayfasına gelen id ile çekilmiş eventi gönderir.
        public System.Web.Mvc.ActionResult GetEditEvent(int Id)
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }
            // gelen id ile data çekilir. Sonra bu data gösterilmek üzere view'e gönderilir.
            calendarEntities dc = new calendarEntities();
            var data = dc.Events.Where(x => x.EventID == Id).FirstOrDefault();

            return View("EditEvent", data);
        }

        // giriş kontrol ediyo. yapmış iste create sayfasına atıyor.
        public System.Web.Mvc.ActionResult GetCreateEvent()
        {
            User currentUser = (User)Session["CurrentUser"];
            if (currentUser == null)
            {
                return View("Login");
            }

            return View("Create");
        }
    }
}
