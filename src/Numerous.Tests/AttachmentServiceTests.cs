using Discord;
using Numerous.Configuration;
using Numerous.Discord;
using Numerous.Services;
using Numerous.Tests.Stubs;

namespace Numerous.Tests;

public sealed class AttachmentServiceTests
{
    private const string AttachmentDirectory = "./attachments";

    private static readonly string[] _testFiles =
    [
        "10001_1_test1.png",
        "10007_1_test2.png",
        "10009_1_test3.png",
        "10009_2_test4.png",
        "10009_3_test5.png",
        "10101_1_test6.png",
        "10101_2_test7.png",
        "10110_1_test8.png",
        "10111_1_test9.png",
        "10112_1_test10.png",
    ];

    [Fact]
    public void GetTargetPath_ReturnsCorrectPath()
    {
        var configService = Substitute.For<IConfigService>();
        configService.Get().ReturnsForAnyArgs(new Config
        {
            AttachmentDirectory = AttachmentDirectory,
        });

        var fileService = Substitute.For<IFileService>();
        fileService.GetFiles(null, null).ReturnsForAnyArgs([]);

        var sut = new AttachmentService(configService, fileService);

        var attachment = Substitute.For<IAttachment>();
        attachment.Url.Returns("https://example.com/test.png");

        var path = sut.GetTargetPath(10001, attachment, 1);

        Path.GetFullPath(path).Should().Be(Path.GetFullPath(Path.Join(AttachmentDirectory, "10001_1_test.png")));
    }

    [Fact]
    public void GetFileAttachments_MessageWithSingleAttachment_ReturnsAttachment()
    {
        var configService = Substitute.For<IConfigService>();
        configService.Get().ReturnsForAnyArgs(new Config
        {
            AttachmentDirectory = AttachmentDirectory,
        });

        IFileService fileService = new StubFileService(_testFiles, AttachmentDirectory);

        var sut = new AttachmentService(configService, fileService);

        var attachments = sut.GetFileAttachments(10001).ToArray();

        attachments.Should().HaveCount(1);
        Path.GetFullPath(attachments[0].Path).Should().Be(Path.GetFullPath(Path.Join(AttachmentDirectory, "10001_1_test1.png")));
        attachments[0].FileName.Should().Be("test1.png");
    }

    [Fact]
    public void GetFileAttachments_MessageWithMultipleAttachments_ReturnsAllAttachments()
    {
        var configService = Substitute.For<IConfigService>();
        configService.Get().ReturnsForAnyArgs(new Config
        {
            AttachmentDirectory = AttachmentDirectory,
        });

        IFileService fileService = new StubFileService(_testFiles, AttachmentDirectory);

        var sut = new AttachmentService(configService, fileService);

        var attachments = sut.GetFileAttachments(10009).ToArray();

        attachments.Should().HaveCount(3);
        Path.GetFullPath(attachments[0].Path).Should().Be(Path.GetFullPath(Path.Join(AttachmentDirectory, "10009_1_test3.png")));
        attachments[0].FileName.Should().Be("test3.png");
        Path.GetFullPath(attachments[1].Path).Should().Be(Path.GetFullPath(Path.Join(AttachmentDirectory, "10009_2_test4.png")));
        attachments[1].FileName.Should().Be("test4.png");
        Path.GetFullPath(attachments[2].Path).Should().Be(Path.GetFullPath(Path.Join(AttachmentDirectory, "10009_3_test5.png")));
        attachments[2].FileName.Should().Be("test5.png");
    }
}
