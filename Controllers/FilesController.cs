using jb_core_webapi.Models;
using jb_core_webapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace jb_core_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IJellyblogDbFileService _fileService;

        public FilesController(IJellyblogDbFileService fileService)
        {
            this._fileService = fileService;
        }

        // GET api/files
        // [HttpGet]
        // [Produces("application/json")]
        // public ActionResult<PaginationResponse<File>> Get()
        // {
        // var l = this._fileService.Get();s
        // var s = l.ToJson();
        // return s;
        // var listOfFiles = this._fileService.Get();
        // return new PaginationResponse<File>(Items: listOfFiles, HasMore: false);
        // }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<PaginationResponse<File>>> Get([FromQuery] FileFindCriteria criteria)
        {
            return await this._fileService.Get(criteria);
        }

        /*
                // GET api/values/5
                [HttpGet("{id}")]
                public ActionResult<string> Get(int id)
                {
                    return "value";
                }

                // POST api/values
                [HttpPost]
                public void Post([FromBody] string value)
                {
                }

                // PUT api/values/5
                [HttpPut("{id}")]
                public void Put(int id, [FromBody] string value)
                {
                }

                // DELETE api/values/5
                [HttpDelete("{id}")]
                public void Delete(int id)
                {
                }
                */
    }
}
