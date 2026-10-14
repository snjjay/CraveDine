using System.Reflection;
using EatKath.API.Controllers;
using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Controllers;

// SurpriseBagController's image endpoints: who may call them (attributes,
// as ASP.NET Core applies them) and what they return. Business rules
// (ownership, validation, files) are covered by SurpriseBagServiceTests.
[TestClass]
public class SurpriseBagControllerTests
{
    private Mock<ISurpriseBagService> _service = null!;
    private SurpriseBagController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new Mock<ISurpriseBagService>(MockBehavior.Strict);
        _controller = new SurpriseBagController(_service.Object);
    }

    private static MethodInfo Action(string name) =>
        typeof(SurpriseBagController).GetMethod(name)!;

    [TestMethod]
    [DataRow(nameof(SurpriseBagController.UploadImage), typeof(HttpPostAttribute))]
    [DataRow(nameof(SurpriseBagController.DeleteImage), typeof(HttpDeleteAttribute))]
    public void ImageEndpoints_ShouldRequireAnOwnerOrAdmin(string action, Type verb)
    {
        var method = Action(action);

        method.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Admin,Owner");
        method.GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();
        typeof(SurpriseBagController).GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();

        var route = (HttpMethodAttribute)method.GetCustomAttribute(verb)!;
        route.Template.Should().Be("{id:int}/image");
    }

    // Adding the image endpoints left the public browsing endpoints public.
    [TestMethod]
    [DataRow(nameof(SurpriseBagController.GetAll))]
    [DataRow(nameof(SurpriseBagController.GetByRestaurant))]
    [DataRow(nameof(SurpriseBagController.GetById))]
    public void PublicEndpoints_ShouldStayAnonymous(string action)
    {
        Action(action).GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
    }

    [TestMethod]
    public async Task UploadImage_ShouldPassTheFileToTheService_AndReturnTheStoredPath()
    {
        var file = new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "File", "photo.png");
        _service.Setup(s => s.UploadImageAsync(7, file)).ReturnsAsync("/uploads/surprisebags/7/image-abc.png");

        var result = await _controller.UploadImage(7, new UploadSurpriseBagImageDto { File = file });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { ImageUrl = "/uploads/surprisebags/7/image-abc.png" });
        _service.VerifyAll();
    }

    [TestMethod]
    public async Task DeleteImage_ShouldReturnNoContent()
    {
        _service.Setup(s => s.DeleteImageAsync(7)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteImage(7);

        result.Should().BeOfType<NoContentResult>();
        _service.VerifyAll();
    }

    // Errors are left to ExceptionMiddleware (404 / 400 with a message).
    [TestMethod]
    public async Task ImageEndpoints_ShouldNotSwallowServiceErrors()
    {
        _service.Setup(s => s.DeleteImageAsync(9999)).ThrowsAsync(new NotFoundException("Surprise Bag not found."));
        _service.Setup(s => s.UploadImageAsync(8, It.IsAny<IFormFile>()))
            .ThrowsAsync(new BusinessRuleException("You are not authorized to change the image of this Surprise Bag."));

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _controller.DeleteImage(9999));
        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _controller.UploadImage(8, new UploadSurpriseBagImageDto { File = Mock.Of<IFormFile>() }));
    }
}
