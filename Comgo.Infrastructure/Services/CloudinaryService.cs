﻿using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Comgo.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Comgo.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly IConfiguration _config;
        public CloudinaryService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> FromBase64ToFile(string base64File, string filename)
        {
            try
            {
                var fileLocation = "";
                if (!string.IsNullOrEmpty(base64File))
                {
                    if (base64File.Contains("https:"))
                    {
                        return base64File;
                    }
                    var imagebytes = Convert.FromBase64String(base64File);
                    if (imagebytes.Length > 0)
                    {
                        string file = Path.Combine(Directory.GetCurrentDirectory(), filename);
                        using (var stream = new FileStream(file, FileMode.Create))
                        {
                            stream.Write(imagebytes, 0, imagebytes.Length);
                            stream.Flush();
                        }
                        fileLocation = file;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
                return fileLocation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> UploadImage(string base64string, string userid)
        {
            try
            {
                Account account = new Account
                {
                    ApiKey = _config["cloudinary:key"],
                    ApiSecret = _config["cloudinary:secret"],
                    Cloud = _config["cloudinary:cloudname"]
                };
                Cloudinary cloudinary = new Cloudinary(account);
                var uploadParams = new ImageUploadParams()
                {
                    //File = new FileDescription(fileLocation)
                    File = new FileDescription(base64string)
                };
                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    throw new Exception($"An error occured while uploading document. {uploadResult.Error.Message}");
                }
                var fileUrl = uploadResult.SecureUrl.AbsoluteUri;
                return fileUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> UploadInvoiceQRCode(string location)
        {
            try
            {
                var fileLocation = Directory.GetCurrentDirectory() + $"\\{location}.jpg";
                Account account = new Account
                {
                    ApiKey = _config["cloudinary:key"],
                    ApiSecret = _config["cloudinary:secret"],
                    Cloud = _config["cloudinary:cloudname"]
                };
                Cloudinary cloudinary = new Cloudinary(account);
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileLocation)
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    throw new Exception("An error occured while uploading document");
                }

                //var fileUrl = uploadResult.Uri.ToString();
                string fileUrl = uploadResult.SecureUri.AbsoluteUri;
                File.Delete(fileLocation);
                return fileUrl;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
