using Sklep.Models.db;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.EntityManager
{
    public class CommentManager
    {
        public static bool AddComment(string login, int productID, string description)
        {
          //  if (CheckExistsCommentFromUser(login, productID))
            //    return false;
            using (sklepEntities db = new sklepEntities())
            {
                Comments comment = new Comments();
                comment.UserID = UserManager.GetUserID(login);
                comment.ProductID = productID;
                comment.Date = DateTime.Now;
                comment.Description = description;
                db.Comments.Add(comment);
                db.SaveChanges();
                return true;
            }
        }
        public static bool CheckExistsCommentFromUser(string login, int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(user => user.UserName == login).Join(db.Comments, user => user.UserID, pr => pr.UserID,
                    (user, pr) => new { pr.UserID, user.UserName, pr.ProductID }).Where(pr => pr.ProductID == productID).Any();
            }
        }
        public static List<CommentViewModel> GetCommentFromProduct(int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                List<CommentViewModel> commentsList = new List<CommentViewModel>();
                var comments = db.Comments.Where(x => x.ProductID == productID);
                foreach(Comments item in comments)
                {
                    CommentViewModel comm = new CommentViewModel();
                    comm.User = UserManager.GetUserLogin(item.UserID);
                    comm.Date = item.Date;
                    comm.Description = item.Description;
                    commentsList.Add(comm);
                }
                return commentsList;
            }
        }
    }
}