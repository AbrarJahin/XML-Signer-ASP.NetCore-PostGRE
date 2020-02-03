﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace XmlSigner.Data.Models
{
    //[Table("Product")]
    public class BaseModel
    {
        [HiddenInput(DisplayValue = false), Display(Name = "ID")]
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public long Id { get; set; }
        [HiddenInput(DisplayValue = false), Display(Name = "First Entry Time")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("CreateTime", TypeName = "TIMESTAMPTZ")]
        public DateTime? CreateTime { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [HiddenInput(DisplayValue = false), Display(Name = "Last Update Time"), Column("LastUpdateTime")]
        public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

        //Soft Delete
        /*
        [HiddenInput(DisplayValue = false), Display(Name = "Is Item Deleated"), Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
        */
        [IgnoreDataMember]
        [HiddenInput(DisplayValue = false)]
        public DateTime? DeletionTime { get; set; }     //DeletionTime not null means the item is deleted

        public void SoftDelete()
        {
            DeletionTime = DateTime.UtcNow;
        }
    }
}
