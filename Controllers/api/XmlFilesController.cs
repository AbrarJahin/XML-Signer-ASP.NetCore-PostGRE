﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XmlSigner.Data;
using XmlSigner.Data.Models;
using XmlSigner.Library;

namespace XmlSigner.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class XmlFilesController : ControllerBase
    {
        private UserManager<IdentityUser<long>> _userManager;
        private readonly ApplicationDbContext _context;

        public XmlFilesController(ApplicationDbContext context, UserManager<IdentityUser<long>> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/XmlFiles
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<long>> UploadXmlFile([FromForm]IFormFile xmlFile, [FromForm]long? previousFileId, [FromForm]string token)    //XmlFile xmlFile
        {
            XmlFile uploadedFile = new XmlFile();
            if (previousFileId == null)
            {
                return BadRequest("Previous File ID not Given");
            }
            else
            {
                uploadedFile.PreviousSignedFile = await _context.XmlFiles
                                                    .Where(xml => xml.Id == previousFileId)
                                                    .Where(xml => xml.DownloadToken == token)
                                                    .Where(xml => xml.DownloadTokenExpirityTime >= DateTime.UtcNow)
                                                    .FirstOrDefaultAsync();
                if(uploadedFile.PreviousSignedFile == null)
                {
                    return BadRequest("Valid Token and previous file ID don't match");
                }
                uploadedFile.PreviousSignedFile.DownloadTokenExpirityTime = DateTime.UtcNow;    //Update Time for marking as used
            }

            if (xmlFile.Length > 0)
            {
                uploadedFile.FileContent = await Adapter.ReadAsStringAsync(xmlFile);
                uploadedFile.FileRealName = xmlFile.FileName;
                uploadedFile.Signer = await _userManager.GetUserAsync(User);
            }
            else
            {
                return BadRequest("A file Should be Uploaded");
            }
            _context.XmlFiles.Add(uploadedFile);
            await _context.SaveChangesAsync();
            return uploadedFile.Id;
        }

        // GET: api/XmlFiles/token/9
        [Authorize]
        [HttpGet("token/{id}")]
        public async Task<string> GetXmlFileDownloadToken(long id)
        {
            //Should add token verification
            XmlFile xmlFile = await _context.XmlFiles.FindAsync(id);
            if (xmlFile == null)
            {
                return "";
            }
            xmlFile.DownloadToken = Guid.NewGuid().ToString();
            xmlFile.DownloadTokenExpirityTime = DateTime.UtcNow.AddMinutes(5);

            _context.XmlFiles.Update(xmlFile);
            await _context.SaveChangesAsync();

            return xmlFile.DownloadToken;
        }

        // GET: api/XmlFiles/asdasd234/9
        [HttpGet("{token}/{id}")]
        public async Task<IActionResult> DownloadXmlFile(long id, string token)
        {
            XmlFile xmlFile = await _context.XmlFiles
                                .Where(xml => xml.Id == id)
                                .Where(xml => xml.DownloadToken == token)
                                .Where(xml => xml.DownloadTokenExpirityTime >= DateTime.UtcNow)
                                .FirstOrDefaultAsync();
            if (xmlFile == null)
            {
                return NoContent();
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(xmlFile.FileContent);
            MemoryStream fileStream = new MemoryStream(byteArray);
            return File(fileStream, "application/ocet-stream", xmlFile.FileRealName);
        }

        // GET: api/XmlFiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<XmlFile>>> GetXmlFiles()
        {
            //Should use automapper - https://code-maze.com/automapper-net-core/
            return await _context.XmlFiles.ToListAsync();
        }

        // Post: api/XmlFiles/5
        [HttpPost("{id}")]
        public async Task<ActionResult<XmlFile>> GetXmlFile(long id)
        {
            var xmlFile = await _context.XmlFiles.FindAsync(id);
            if (xmlFile == null)
            {
                return NotFound();
            }
            return xmlFile;
        }

        // PUT: api/XmlFiles/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutXmlFile(long id, XmlFile xmlFile)
        {
            if (id != xmlFile.Id)
            {
                return BadRequest();
            }

            _context.Entry(xmlFile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!XmlFileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/XmlFiles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<XmlFile>> DeleteXmlFile(long id)
        {
            var xmlFile = await _context.XmlFiles.FindAsync(id);
            if (xmlFile == null)
            {
                return NotFound();
            }

            _context.XmlFiles.Remove(xmlFile);
            await _context.SaveChangesAsync();

            return xmlFile;
        }

        private bool XmlFileExists(long id)
        {
            return _context.XmlFiles.Any(e => e.Id == id);
        }
    }
}
