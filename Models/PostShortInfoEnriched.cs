using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jb_core_webapi.Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace jb_core_webapi.Models
{
    public class PostShortInfoEnriched : PostShortInfo, IEnrichable<IJellyblogDbFileService>
    {

        public new JbFileInfo TitleImg { get; private set; }

        public async Task Enrich(IJellyblogDbFileService fileService)
        {
            if(string.IsNullOrEmpty(base.TitleImg))
            {
                return;
            }

            if(!ObjectId.TryParse(base.TitleImg, out ObjectId titleImgObjectId))
            {
                throw new ApplicationException("Cannot parse object id");
            }

            var titleImgFileInfo = await fileService.Get(titleImgObjectId);
            this.TitleImg = titleImgFileInfo;
        }
    }
}
