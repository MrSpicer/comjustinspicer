using Microsoft.Extensions.Logging;
using AutoMapper;
using NUnit.Framework;
using Comjustinspicer.CMS.Data;

namespace Comjustinspicer.Tests;

public class MappingProfileTests
{
	[Test]
	public void MappingConfiguration_IsValid()
	{
		var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), LoggerFactory.Create(builder => builder.AddConsole()));
		config.AssertConfigurationIsValid();
	}
}
