﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProdFloor.Controllers;
using ProdFloor.Models;
using ProdFloor.Models.ViewModels;
using Xunit;

namespace ProdFloor.Tests
{
    public class JobControllerTests
    {
        [Fact]
        public void Can_Paginate()
        {
            // Arrange
            Mock<IJobRepository> mock = new Mock<IJobRepository>();
            mock.Setup(m => m.Jobs).Returns((new Job[] {
                new Job {JobID = 1, Name = "P1"},
                new Job {JobID = 2, Name = "P2"},
                new Job {JobID = 3, Name = "P3"},
                new Job {JobID = 4, Name = "P4"},
                new Job {JobID = 5, Name = "P5"}
            }).AsQueryable<Job>());

            JobController controller = new JobController(mock.Object);
            controller.PageSize = 3;

            // Act
            JobsListViewModel result =
            controller.List(null, 2).ViewData.Model as JobsListViewModel;

            // Assert
            Job[] prodArray = result.Jobs.ToArray();
            Assert.True(prodArray.Length == 2);
            Assert.Equal("P4", prodArray[0].Name);
            Assert.Equal("P5", prodArray[1].Name);
        }

        [Fact]
        public void Can_Send_Pagination_View_Model()
        {
            // Arrange
            Mock<IJobRepository> mock = new Mock<IJobRepository>();
            mock.Setup(m => m.Jobs).Returns((new Job[] {
                new Job {JobID = 1, Name = "P1"},
                new Job {JobID = 2, Name = "P2"},
                new Job {JobID = 3, Name = "P3"},
                new Job {JobID = 4, Name = "P4"},
                new Job {JobID = 5, Name = "P5"}
            }).AsQueryable<Job>());

            // Arrange
            JobController controller =
            new JobController(mock.Object) { PageSize = 3 };

            // Act
            JobsListViewModel result =
            controller.List(null, 2).ViewData.Model as JobsListViewModel;

            // Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.Equal(2, pageInfo.CurrentPage);
            Assert.Equal(3, pageInfo.ItemsPerPage);
            Assert.Equal(5, pageInfo.TotalItems);
            Assert.Equal(2, pageInfo.TotalPages);
        }

        [Fact]
        public void Can_Filter_Products()
        {
            // Arrange
            // - create the mock repository
            Mock<IJobRepository> mock = new Mock<IJobRepository>();
            mock.Setup(m => m.Jobs).Returns((new Job[] {
                new Job {JobID = 1, Name = "P1", JobType = "Cat1"},
                new Job {JobID = 2, Name = "P2", JobType = "Cat2"},
                new Job {JobID = 3, Name = "P3", JobType = "Cat1"},
                new Job {JobID = 4, Name = "P4", JobType = "Cat2"},
                new Job {JobID = 5, Name = "P5", JobType = "Cat3"}
            }).AsQueryable<Job>());

            // Arrange - create a controller and make the page size 3 items
            JobController controller = new JobController(mock.Object);
            controller.PageSize = 3;

            // Action
            Job[] result =
            (controller.List("Cat2", 1).ViewData.Model as JobsListViewModel)
            .Jobs.ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.True(result[0].Name == "P2" && result[0].JobType == "Cat2");
            Assert.True(result[1].Name == "P4" && result[1].JobType == "Cat2");
        }

        [Fact]
        public void Generate_Category_Specific_Product_Count()
        {
            // Arrange
            Mock<IJobRepository> mock = new Mock<IJobRepository>();
            mock.Setup(m => m.Jobs).Returns((new Job[] {
                new Job {JobID = 1, Name = "P1", JobType = "Cat1"},
                new Job {JobID = 2, Name = "P2", JobType = "Cat2"},
                new Job {JobID = 3, Name = "P3", JobType = "Cat1"},
                new Job {JobID = 4, Name = "P4", JobType = "Cat2"},
                new Job {JobID = 5, Name = "P5", JobType = "Cat3"}
            }).AsQueryable<Job>());

            JobController target = new JobController(mock.Object);
            target.PageSize = 3;

            Func<ViewResult, JobsListViewModel> GetModel = result =>
            result?.ViewData?.Model as JobsListViewModel;

            // Action
            int? res1 = GetModel(target.List("Cat1"))?.PagingInfo.TotalItems;
            int? res2 = GetModel(target.List("Cat2"))?.PagingInfo.TotalItems;
            int? res3 = GetModel(target.List("Cat3"))?.PagingInfo.TotalItems;
            int? resAll = GetModel(target.List(null))?.PagingInfo.TotalItems;

            // Assert
            Assert.Equal(2, res1);
            Assert.Equal(2, res2);
            Assert.Equal(1, res3);
            Assert.Equal(5, resAll);
        }
    }
}
