using jb_core_webapi.Models;
using jb_core_webapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
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
        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            StringValues fileContext;
            StringValues fileDescription;


            if (Request.Form.Files.Count != 1)
            {
                // allow one attachment
                return new BadRequestResult();
            }
            if (!Request.Form.TryGetValue("context", out fileContext))
            {
                // not set context - required
                return new BadRequestResult();
            }

            var oneUploadFile = Request.Form.Files[0];
            using (var oneStream = oneUploadFile.OpenReadStream())
            {
                var justStored = await _fileService.Upload(
                    stream: oneStream,
                    filename: _generateFileName(),
                    contentType: oneUploadFile.ContentType,
                    meta: new JbFileMetadata
                    {
                        Context = fileContext.ToString(),
                        Description = Request.Form.TryGetValue("description", out fileDescription) ? fileDescription.ToString() : null,
                        Height = Request.Form.TryGetValue("height", out fileDescription) ? fileDescription.ToString() : null,
                        OriginalName = Request.Form.TryGetValue("originalname", out fileDescription) ? fileDescription.ToString() : null,
                        PostId = Request.Form.TryGetValue("postId", out fileDescription) ? fileDescription.ToString() : null,
                        SrcSetTag = Request.Form.TryGetValue("srcsetTag", out fileDescription) ? fileDescription.ToString() : null,
                        Width = Request.Form.TryGetValue("width", out fileDescription) ? fileDescription.ToString() : null
                    });
                dynamic result = new ExpandoObject();
                AddProperty(result, oneUploadFile.Name, new JbFileInfo[] { justStored });
                return new ObjectResult(result);
                //return justStored;
            }

            throw new NotImplementedException();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<PaginationResponse<JbFileInfo>>> Get([FromQuery] FileFindCriteria criteria)
        {
            // FIXME GridFSFileInfo curly serializes. We have to use more simple model to pass
            var result = await this._fileService.Get(criteria);
            return result;
        }

        private static string _generateFileName()
        {
            var bytes = new byte[16];
            var rnd = new Random();
            var stringBuilder = new StringBuilder();
            rnd.NextBytes(bytes);
            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("x"));
            }
            return stringBuilder.ToString();
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                expandoDict.Add(propertyName, propertyValue);
            }
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
