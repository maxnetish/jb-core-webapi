using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jb_core_webapi.Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace jb_core_webapi.Models
{
    public class PostShortInfoEnriched : PostShortInfo
    {
        public new JbFileInfo TitleImg { get; private set; }

        public PostShortInfoEnriched()
        {

        }

        public PostShortInfoEnriched(PostShortInfo postShortInfo, JbFileInfo titleImg)
        {
            Brief = postShortInfo.Brief;
            CreateDate = postShortInfo.CreateDate;
            Id = postShortInfo.Id;
            PubDate = postShortInfo.PubDate;
            Status = postShortInfo.Status;
            Title = postShortInfo.Title;
            TitleImg = titleImg;
            UpdateDate = postShortInfo.UpdateDate;
        }
       
        //public async Task Enrich(IJellyblogDbFileService fileService)
        //{
        //    if(string.IsNullOrEmpty(base.TitleImg))
        //    {
        //        return;
        //    }

        //    if(!ObjectId.TryParse(base.TitleImg, out ObjectId titleImgObjectId))
        //    {
        //        throw new ApplicationException("Cannot parse object id");
        //    }

        //    var titleImgFileInfo = await fileService.Get(titleImgObjectId);
        //    this.TitleImg = titleImgFileInfo;
        //}
    }
}
