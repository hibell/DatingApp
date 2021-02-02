using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // Uses DI to get data from database. Controller has two endpoints:
    //  1. An endpoint to get all users in the DB
    //  2. An endpoint to get a specific user in DB
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        // IEnumerable allows us to use simple iteration over a collection of a specified type.
        // While we could use List<>, it has too many features such as sorting, adding, etc. that we do not need.
        // We just need to iterate over a list for `GetUsers()`.

        // Uses System.Linq feature ToList().

        // This code is not ideal because it is synchronous and blocks the thread handling the request until the DB req
        // is fulfilled.
        /*
        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers() {
            return _context.Users.ToList();
        }

        // Endpoint: /api/users/3 -> gets user with id = 3
        [HttpGet("{id}")]
        public ActionResult<AppUser> GetUser(int id) {
            return _context.Users.Find(id);
        }
        */

        // The code can be made ansynchronous and tell the thread when it gets to the DB, pass it to another thread.
        // That new thread will deal with getting the data and in the meantime, the request thread can serve other
        // requests.

        // To make code asynchronous:
        //  1. Specify async keyword after method scope
        //  2. Wrap return in a Task<>.
        //  3. Mark blocking code with await.
        //  4. Convert to aysnc methods.

        // ToListAsync() provided by System.Collections.Generic.
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() {
            return await _context.Users.ToListAsync();
        }

        // Endpoint: /api/users/3 -> gets user with id = 3
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id) {
            return await _context.Users.FindAsync(id);
        }
    }
}