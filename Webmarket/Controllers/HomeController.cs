using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webmarket.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace Webmarket.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        private bool IsAdmin { get => db.Users.Find(User.Identity.GetUserId()).Admin; }

        public ActionResult Index()
        {
            return View(db.Products);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            if (!IsAdmin) return Redirect("/Home/AccessError");
            return View();
        }
        [HttpPost]
        public ActionResult AddProduct(Product product)
        {
            if (!IsAdmin) return Redirect("/Home/AccessError");
            db.Products.Add(product);
            db.SaveChanges();
            return Redirect("/Home/Index");
        }

        [HttpGet]
        public ActionResult EditProduct(int? id)
        {
            if(!IsAdmin) return Redirect("/Home/AccessError");
            if (id == null)
            {
                return HttpNotFound();
            }
            Product product = db.Products.Find(id);
            if (product != null)
            {
                return View(product);
            }
            return HttpNotFound();
        }
        [HttpPost]
        public ActionResult EditProduct(Product product)
        {
            if (!IsAdmin) return Redirect("/Home/AccessError");
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect("/Home/Index");
        }

        [HttpGet]
        public ActionResult ToBasket(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Product product = db.Products.Find(id);
            if (product != null)
            {
                return View(product);
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult ToBasket(Product product)
        {
            int max = db.Products.Find(product.Id).Number;

            if (product.Number > max)
            {
                ModelState.AddModelError(nameof(product.Number), "Данный товар не наличествует в таком количестве");
            }
            if (ModelState.IsValid)
            {
                List<Product> products;
                if (Session[User.Identity.GetUserId()] != null)
                {
                    products = (List<Product>)Session[User.Identity.GetUserId()];
                    products.Add(product);
                }
                else
                {
                    products = new List<Product>() { product };                   
                }
                Session[User.Identity.GetUserId()] = products;
                return Redirect("/Home/Index");
            }
            return View(product);
        }
       
        public ActionResult Basket()
        {
            if (Session[User.Identity.GetUserId()] != null)
            {
                //int price = 0;
                List<Product> products = Session[User.Identity.GetUserId()] as List<Product>;
                //foreach(var prod in products)
                //{
                //    price += prod.Price*prod.Number;
                //}
                
                //Session[User.Identity.GetUserId()+"Price"] = price;
                return View(products);
            }
            return Redirect("/Home/Index");
        }

        
        public ActionResult Purchase()
        {
            var id = User.Identity.GetUserId();
            var price = 0;
            List<Product> products= Session[id] as List<Product>;
            foreach (var prod in products)
            {
                var edited = db.Products.Find(prod.Id);
                edited.Number -= prod.Number;
                db.Entry(edited).State = EntityState.Modified;
                db.SaveChanges();
                price += prod.Number * prod.Price;
            }
            var order = new Order() { User = id,Price=price };
            db.Orders.Add(order);
            db.SaveChanges();
            return View();
        }

        public ActionResult Orders()
        {
            if (!IsAdmin) return Redirect("/Home/AccessError");
            return View(db.Orders);
        }
    }
}