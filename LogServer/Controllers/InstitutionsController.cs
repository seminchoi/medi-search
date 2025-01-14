using LogServer.Models;
using LogServer.Services;
using LogServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LogServer.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class InstitutionsController : ControllerBase
{
    private readonly InstitutionService _institutionService;

    public InstitutionsController(InstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    /// <summary>
    /// 지정된 반경 내의 현재 영업 중인 기관을 검색합니다.
    /// </summary>
    /// <param name="latitude">위도 (예: 37.5544)</param>
    /// <param name="longitude">경도 (예: 126.9365)</param>
    /// <param name="radiusInMeters">검색 반경(미터) (기본값: 1000m)</param>
    /// <returns>검색된 기관 목록</returns>
    /// <response code="200">검색 성공</response>
    /// <response code="400">잘못된 요청</response>
    [HttpGet("hospitals")]
    [ProducesResponseType(typeof(InstitutionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<InstitutionsDto> GetOpenHospitalsInRange(
        [Required][FromQuery] double latitude = 37.5544,
        [Required][FromQuery] double longitude = 126.9365,
        [FromQuery] double radiusInMeters = 1000)
    {
        try
        {
            var institutions = _institutionService.SearchOpenHospitalsInRange(
                latitude, longitude, radiusInMeters, "Hospital");
            

            return Ok(new InstitutionsDto { Institutions = institutions });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}