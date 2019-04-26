using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jb_core_webapi.Services;

namespace jb_core_webapi.Controllers
{
    [Route("fs")]
    public class FileStoreController : Controller
    {
        public FileStoreController(IJellyblogDbFileService fileService)
        {
            _fileService = fileService;
        }

        private IJellyblogDbFileService _fileService;

        [Route("{filename:maxlength(128)}")]
        public async Task<IActionResult> DownloadByName(string filename)
        {
            // filename is "virtual" auto random generated filename, not originalFilename
            try
            {
                var gridFsStream = await _fileService.OpenStreamByFilename(filename);
                var fileStreamResult = new FileStreamResult(gridFsStream, gridFsStream.FileInfo.ContentType);
                var originalNameBsonValue = gridFsStream.FileInfo.Metadata.GetValue("originalName");

                if (!originalNameBsonValue.IsBsonNull)
                {
                    fileStreamResult.FileDownloadName = originalNameBsonValue.AsString;
                }

                return fileStreamResult;
            }
            catch (MongoDB.Driver.GridFS.GridFSFileNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}