using System.ComponentModel.DataAnnotations;
using LogServer.Dtos;
using LogServer.Services;
using Microsoft.AspNetCore.Mvc;

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
    /// 현재 요일과 시간을 직접 입력하여 API를 테스트합니다. 
    /// </summary>
    /// <param name="latitude">위도 (예: 37.5544)</param>
    /// <param name="longitude">경도 (예: 126.9365)</param>
    /// <param name="radiusInMeters">검색 반경(미터) (기본값: 1000m)</param>
    /// <param name="type">기관 타입 (Hospital, DrugStore) (기본값: Hospital)</param>
    /// <param name="time">검색 시간 (포맷 HHmm)(기본값: 1500)</param>
    /// <param name="dayOfWeek">검색요일 (기본값: Tuesday)</param>
    /// <returns>검색된 기관 목록</returns>
    /// <response code="200">검색 성공</response>
    /// <response code="400">잘못된 요청</response>
    [HttpGet("testapi")]
    [ProducesResponseType(typeof(InstitutionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<InstitutionsDto> GetOpenDrugStoresInRange(
        [Required][FromQuery] double latitude = 37.5544,
        [Required][FromQuery] double longitude = 126.9365,
        [FromQuery] double radiusInMeters = 1000,
        [FromQuery] string type = "Hospital",
        [FromQuery] string time = "1500",
        [FromQuery] string dayOfWeek = "Tuesday"
        )
    {
        try
        {
            var dayOfWeekEnum = Enum.Parse<DayOfWeek>(dayOfWeek);

            var institutions = _institutionService.SearchOpenInstitutionsInRange(
                latitude, longitude, radiusInMeters, type, time, dayOfWeekEnum);
            

            return Ok(institutions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
    /// <summary>
    /// 지정된 반경 내의 현재 영업 중인 병원을 검색합니다.
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
            var institutions = _institutionService.SearchOpenInstitutionsInRange(
                latitude, longitude, radiusInMeters, "Hospital");
            

            return Ok(institutions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// 지정된 반경 내의 현재 영업 중인 약국을 검색합니다.
    /// </summary>
    /// <param name="latitude">위도 (예: 37.5544)</param>
    /// <param name="longitude">경도 (예: 126.9365)</param>
    /// <param name="radiusInMeters">검색 반경(미터) (기본값: 1000m)</param>
    /// <returns>검색된 기관 목록</returns>
    /// <response code="200">검색 성공</response>
    /// <response code="400">잘못된 요청</response>
    [HttpGet("drugstores")]
    [ProducesResponseType(typeof(InstitutionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<InstitutionsDto> GetOpenDrugStoresInRange(
        [Required][FromQuery] double latitude = 37.5544,
        [Required][FromQuery] double longitude = 126.9365,
        [FromQuery] double radiusInMeters = 1000)
    {
        try
        {
            var institutions = _institutionService.SearchOpenInstitutionsInRange(
                latitude, longitude, radiusInMeters, "DrugStore");
            

            return Ok(institutions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}