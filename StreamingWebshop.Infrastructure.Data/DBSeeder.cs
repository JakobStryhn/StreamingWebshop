﻿using System;
using System.Collections.Generic;
using System.Text;
using StreamingWebshop.Infrastructure.Data;
using StreamingWebshop.Core.Entity;

namespace StreamingWebshop.Infrastructure.Data
{
    public class DBSeeder
    {
        public static void SeedDB(Context ctx)
        {
            //This ensures that the DB is deleted. before seeding
            //MAKE SURE TO BE UNDER THE SECTION "env.IsDevelopment()" 
            //else it can delete complete production DB !!!DØD BABY!!!

            ctx.Database.EnsureDeleted();

            //Ensures the DB is created with the startup customers and orders
            //Seeds the Data into the DB - stacks with each startup
            ctx.Database.EnsureCreated();

            //Fake Products
            var prod1 = ctx.Products.Add(new Product()
            {
                Name = "Greenscreen",
                Description = "Greenscreen",
                RetailPrice = 4000.00,
                WholeSalePrice = 3698.99,
                Category = "Greenscreen",
                Stock = 20,
                PicUrl = "EXAMPLEURL.jpg"
            }).Entity;

            var prod2 = ctx.Products.Add(new Product()
            {
                Name = "WebCam",
                Description = "webcam",
                RetailPrice = 850.95,
                WholeSalePrice = 800.00,
                Category = "Webcam",
                Stock = 20,
                PicUrl = "EXAMPLEURL.jpg"
            }).Entity;

            var prod3 = ctx.Products.Add(new Product()
            {
                Name = "webcam",
                Description = "Webcam",
                RetailPrice = 2299.95,
                WholeSalePrice = 2000.00,
                Category = "Webcam",
                Stock = 20,
                PicUrl = "EXAMPLEURL.jpg"
            }).Entity;
            ctx.SaveChanges();
        }
        
    }
}
