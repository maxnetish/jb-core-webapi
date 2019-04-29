using jb_core_webapi.Models;
using jb_core_webapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace jb_core_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly int _maximumFilesInOneRequest = 8;
        private readonly IJellyblogDbFileService _fileService;

        public FilesController(IJellyblogDbFileService fileService)
        {
            this._fileService = fileService;
        }

        /**
         *  returns structure:
         *  {
         *      [FieldName1]: [jbFIleInfo1, jbFileInfo2, ...],
         *      [FieldName2]: [jbFIleInfo3, jbFileInfo4, ...]
         *      ...
         *  }
         */
        [Authorize(Roles = "admin")]
        [Route("upload")]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Upload()
        {

            if (Request.Form.Files.Count == 0 || Request.Form.Files.Count > _maximumFilesInOneRequest)
            {
                // allow {1 - 8} attachments
                return new BadRequestResult();
            }
            if (!Request.Form.TryGetValue("context", out StringValues fileContext))
            {
                // not set context - required field
                return new BadRequestResult();
            }

            var resultMap = new Dictionary<string, List<JbFileInfo>>();

            foreach (var oneUploadFile in Request.Form.Files)
            {
                using (var oneStream = oneUploadFile.OpenReadStream())
                {
                    var justStored = await _fileService.Upload(
                        stream: oneStream,
                        filename: _generateFileName(),
                        contentType: oneUploadFile.ContentType,
                        meta: _createFileMetadataFromMultipartFormData(Request.Form, oneUploadFile)
                        );
                    _addValueToResultMap(resultMap, oneUploadFile.Name, justStored);
                }
            }
            return new ObjectResult(resultMap);
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

        private static JbFileMetadata _createFileMetadataFromMultipartFormData(IFormCollection requestForm, IFormFile requestFile)
        {
            return new JbFileMetadata
            {
                Context = requestForm.TryGetValue("context", out StringValues formElmValue) ? formElmValue.ToString() : null,
                Description = requestForm.TryGetValue("description", out formElmValue) ? formElmValue.ToString() : null,
                Height = requestForm.TryGetValue("height", out formElmValue) ? formElmValue.ToString() : null,
                OriginalName = String.IsNullOrEmpty(requestFile.FileName) ? (requestForm.TryGetValue("originalname", out formElmValue) ? formElmValue.ToString() : null) : requestFile.FileName,
                PostId = requestForm.TryGetValue("postId", out formElmValue) ? formElmValue.ToString() : null,
                SrcSetTag = requestForm.TryGetValue("srcsetTag", out formElmValue) ? formElmValue.ToString() : null,
                Width = requestForm.TryGetValue("width", out formElmValue) ? formElmValue.ToString() : null
            };
        }

        /**
         * to produce response object with dynamic properties kind of:
         * {
         *      propertyName1: [propertyValue1, propertyValue2, ...],
         *      propertyName2: [propertyValue3, ...],
         *      ...
         * }
         */ 
        private static void _addValueToResultMap<TPropValue>(Dictionary<string, List<TPropValue>> resultMap, string propertyName, TPropValue propertyValue)
        {
            if(!resultMap.ContainsKey(propertyName))
            {
                resultMap.Add(propertyName, new List<TPropValue>());
            }
            resultMap[propertyName].Add(propertyValue);
        }
    }
}
