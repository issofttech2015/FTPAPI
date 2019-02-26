using FTPAPI.Common_Controle;
using FTPAPI.Models;
using Newtonsoft.Json;
using Save_Files_Database_EF_MVC.Context;
using Save_Files_Database_EF_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace FTPAPI.Controllers
{
  [RoutePrefix("API")]
  public class FTPServiceController : ApiController
  {
    [Route("GetDirectoryDetails")]
    [HttpPost]
    public Response<CoustomFile> GetDirectoryDetails(RequestDirectoryDetalis requestDirectoryDetalis)
    {
      Response<CoustomFile> responseFileList = new Response<CoustomFile>();
      if (requestDirectoryDetalis != null)
      {
        try
        {
          List<CoustomFile> coustomFileList = new List<CoustomFile>();
          string ftp = requestDirectoryDetalis.URL == "" ? "ftp://" + Common_Methods.getClientIp() + "/" : requestDirectoryDetalis.URL + "/";
          FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp);
          request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

          request.Credentials = new NetworkCredential(requestDirectoryDetalis.UserName, requestDirectoryDetalis.Password);
          request.UsePassive = true;
          request.UseBinary = true;
          request.EnableSsl = false;

          //Fetch the Response and read it using StreamReader.
          FtpWebResponse response = (FtpWebResponse)request.GetResponse();
          List<string> entries = new List<string>();
          using (StreamReader reader = new StreamReader(response.GetResponseStream()))
          {
            //Read the Response as String and split using New Line character.
            entries = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
          }
          response.Close();
          bool isDirectory;
          decimal size = 0;
          foreach (string entry in entries)
          {
            string[] splits = entry.Split(new string[] { " ", }, StringSplitOptions.RemoveEmptyEntries);
            isDirectory = splits[2].Substring(1, 1).ToLower() == "d" ? true : false;

            size = isDirectory ? 0 : decimal.Parse(splits[2]) / 1024;

            coustomFileList.Add(new CoustomFile()
            {
              Name = getFileName(splits),
              Date = splits[0],
              Size = size,
              Time = splits[1]
            });
          }
          responseFileList.data = coustomFileList;
          responseFileList.status = Convert.ToInt32(HttpStatusCode.OK);
          responseFileList.message = "Success";
          return responseFileList;
        }
        catch
        {
          responseFileList.status = Convert.ToInt32(HttpStatusCode.InternalServerError);
          responseFileList.message = "Error";
          return responseFileList;
        }
      }
      else
      {
        responseFileList.status = Convert.ToInt32(HttpStatusCode.BadRequest);
        responseFileList.message = "Error";
        return responseFileList;
      }
    }

    private static string getFileName(string[] splits)
    {
      var name = "";
      for (int i = 3; i < splits.Length; i++)
      {
        name += splits[i] + " ";
      }
      return name.Trim();
    }


    [Route("GetDownloadFile")]
    [HttpPost]
    public HttpResponseMessage GetDownloadFile(RequestDownloadFile requestDownloadFile)
    {
      if (requestDownloadFile != null)
      {
        try
        {
          string ftp = requestDownloadFile.URL == "" ? "ftp://" + Common_Methods.getClientIp() + "/" : requestDownloadFile.URL + "/";

          string ftpFolder = requestDownloadFile.FolderName;
          string ftpFile = requestDownloadFile.FileName;

          FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp + ftpFolder + ftpFile);
          request.Method = WebRequestMethods.Ftp.DownloadFile;

          request.Credentials = new NetworkCredential(requestDownloadFile.UserName, requestDownloadFile.Password);
          request.UsePassive = true;
          request.UseBinary = true;
          request.EnableSsl = false;


          FtpWebResponse response = (FtpWebResponse)request.GetResponse();
          HttpResponseMessage result;
          using (MemoryStream stream = new MemoryStream())
          {
            //Download the File.

            response.GetResponseStream().CopyTo(stream);
            //Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //Response.BinaryWrite(stream.ToArray());
            //Response.End();

            result = new HttpResponseMessage(HttpStatusCode.OK)
            {
              Content = new ByteArrayContent(stream.ToArray())
            };

            result.Content.Headers.ContentDisposition =
              new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
              {
                FileName = ftpFile,
              };
            var contentType = MimeMapping.GetMimeMapping(Path.GetExtension(ftpFile));
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);

          }
          return result;
        }
        catch
        {
          return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
      }
      else
      {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
      }
    }

    [Route("UploadFile")]
    [HttpPost]
    public HttpResponseMessage UploadFile(RequestUploadFile requestUploadFile)
    {
      if (requestUploadFile != null)
      {
        try
        {
          string ftp = requestUploadFile.URL == "" ? "ftp://" + Common_Methods.getClientIp() + "/" : requestUploadFile.URL + "/";

          string ftpFolder = requestUploadFile.FolderName;
          string ftpFile = requestUploadFile.FileName;

          FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp + ftpFolder + ftpFile);
          request.Method = WebRequestMethods.Ftp.UploadFile;

          //Enter FTP Server credentials.
          request.Credentials = new NetworkCredential("neel", "12345");
          request.ContentLength = requestUploadFile.FileContentLength;
          request.UsePassive = true;
          request.UseBinary = true;
          request.EnableSsl = false;


          using (Stream requestStream = request.GetRequestStream())
          {
            requestStream.Write(StringToByteArray(requestUploadFile.FileData), 0, requestUploadFile.FileContentLength);
            requestStream.Close();
          }

          FtpWebResponse response = (FtpWebResponse)request.GetResponse();


          HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

          return result;
        }
        catch
        {
          return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
      }
      else
      {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
      }
    }

    public static byte[] StringToByteArray(string hex)
    {
      return Enumerable.Range(0, hex.Length)
                       .Where(x => x % 2 == 0)
                       .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                       .ToArray();
    }

    [Route("CheckServer")]
    [HttpPost]
    public Response<APIConfig> CheckServer(APIConfig aPIConfig)
    {
      Response<APIConfig> responseAPIConfig = new Response<APIConfig>();
      if (aPIConfig != null)
      {
        try
        {
          List<APIConfig> aPIConfigList = new List<APIConfig>();

          aPIConfigList.Add(new APIConfig()
          {
            ip = Common_Methods.getClientIp(),
          });

          responseAPIConfig.data = aPIConfigList;
          responseAPIConfig.status = Convert.ToInt32(HttpStatusCode.OK);
          responseAPIConfig.message = "Success";
          return responseAPIConfig;
        }

        catch (Exception ex)
        {
          responseAPIConfig.status = Convert.ToInt32(HttpStatusCode.InternalServerError);
          responseAPIConfig.message = ex.Message;
          return responseAPIConfig;
        }
      }
      else
      {
        responseAPIConfig.status = Convert.ToInt32(HttpStatusCode.BadRequest);
        responseAPIConfig.message = "Error";
        return responseAPIConfig;
      }
    }

    [Route("GetFiles")]
    [HttpPost]
    public Response<ResposTblFile> GetFiles()
    {
      Response<ResposTblFile> responseTblFiles = new Response<ResposTblFile>();
      try
      {
        List<ResposTblFile> files = currentListOfFiles();

        responseTblFiles.data = files;
        responseTblFiles.status = Convert.ToInt32(HttpStatusCode.OK);
        responseTblFiles.message = "Success";
        return responseTblFiles;
      }
      catch (Exception ex)
      {
        responseTblFiles.status = Convert.ToInt32(HttpStatusCode.InternalServerError);
        responseTblFiles.message = ex.Message;
        return responseTblFiles;
      }

    }

    private static List<ResposTblFile> currentListOfFiles()
    {
      TblFileDbContext tblFileDbContext = new TblFileDbContext();

      List<ResposTblFile> files; //= new List<TblFile>();

      files = (from file in tblFileDbContext.TblFile.ToList()
               select new ResposTblFile
               {
                 TblFileID = file.TblFileID,
                 ContentType = file.ContentType,
                 Name = file.Name,
                 //Data = HttpUtility.UrlEncode(Convert.ToBase64String(file.Data))
               }).ToList();
      return files;
    }

    [Route("UploadFiles")]
    [HttpPost]
    public Response<ResposTblFile> UploadFiles(ResposTblFile resposTblFile)
    {
      Response<ResposTblFile> responseTblFiles = new Response<ResposTblFile>();
      if (resposTblFile != null)
      {
        try
        {
          TblFileDbContext tblFileDbContext = new TblFileDbContext();

          if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/img/") + resposTblFile.Name))
          {
            responseTblFiles.message = "File has already Exist";
          }
          else
          {
            TblFile tblFile = new TblFile()
            {
              Name = resposTblFile.Name,
              ContentType = resposTblFile.ContentType,
              //Data = imageByte
            };
            //byte[] imageByte = Convert.FromBase64String(HttpUtility.UrlDecode(resposTblFile.Data));
            File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath("~/img/") + resposTblFile.Name, resposTblFile.Data);

            tblFileDbContext.TblFile.Add(tblFile);
            tblFileDbContext.SaveChanges();
            responseTblFiles.message = "Success";
          }

          List<ResposTblFile> files = currentListOfFiles();

          responseTblFiles.data = files;
          responseTblFiles.status = Convert.ToInt32(HttpStatusCode.OK);
          return responseTblFiles;
        }
        catch (Exception ex)
        {
          responseTblFiles.status = Convert.ToInt32(HttpStatusCode.InternalServerError);
          responseTblFiles.message = ex.Message;
          return responseTblFiles;
        }
      }
      else
      {
        responseTblFiles.status = Convert.ToInt32(HttpStatusCode.BadRequest);
        responseTblFiles.message = "Error";
        return responseTblFiles;
      }
    }

    [Route("DeletedFiles")]
    [HttpPost]
    public Response<ResposTblFile> DeletedFiles(ResposTblFile resposTblFile)
    {
      Response<ResposTblFile> responseTblFiles = new Response<ResposTblFile>();
      if (resposTblFile != null)
      {
        try
        {
          TblFileDbContext tblFileDbContext = new TblFileDbContext();
          TblFile removeFile = tblFileDbContext.TblFile.Where(p => p.TblFileID == resposTblFile.TblFileID).FirstOrDefault();

          using (tblFileDbContext = new TblFileDbContext())
          {
            tblFileDbContext.Entry(removeFile).State = EntityState.Deleted;
            tblFileDbContext.SaveChanges();
          }

          if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/img/") + resposTblFile.Name))
          {
            File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/img/") + resposTblFile.Name);
          }

          List<ResposTblFile> files = currentListOfFiles();

          responseTblFiles.data = files;
          responseTblFiles.status = Convert.ToInt32(HttpStatusCode.OK);
          responseTblFiles.message = "Success";
          return responseTblFiles;
        }
        catch (Exception ex)
        {
          responseTblFiles.status = Convert.ToInt32(HttpStatusCode.InternalServerError);
          responseTblFiles.message = ex.Message;
          return responseTblFiles;
        }
      }
      else
      {
        responseTblFiles.status = Convert.ToInt32(HttpStatusCode.BadRequest);
        responseTblFiles.message = "Error";
        return responseTblFiles;
      }
    }


  }
}
