param(
    [string]$DeviceId = "SIM808_01",
    [string]$ApiBaseUrl = "https://bus-tracking-api-puvv.onrender.com",
    [string]$GpxPath = "C:\Users\User\Desktop\cap\Monday Morning Track.gpx",
    [ValidateRange(1, 3600)]
    [int]$IntervalSeconds = 2,
    [ValidateRange(2, 1000)]
    [int]$Count = 60,
    [ValidateRange(0, 300)]
    [double]$Speed = 45
)

if (-not (Test-Path -LiteralPath $GpxPath)) {
    throw "GPX file not found: $GpxPath"
}

[xml]$gpx = Get-Content -LiteralPath $GpxPath
$trackPoints = @($gpx.SelectNodes("//*[local-name()='trkpt']"))

if ($trackPoints.Count -lt 2) {
    throw "The GPX file must contain at least two track points."
}

$culture = [System.Globalization.CultureInfo]::InvariantCulture
$route = for ($i = 0; $i -lt $Count; $i++) {
    $sourceIndex = [Math]::Round($i * ($trackPoints.Count - 1) / ($Count - 1))
    $point = $trackPoints[$sourceIndex]

    [pscustomobject]@{
        Latitude = [double]::Parse($point.lat, $culture)
        Longitude = [double]::Parse($point.lon, $culture)
        SourceIndex = $sourceIndex
    }
}

function Get-Heading {
    param($From, $To)

    $lat1 = $From.Latitude * [Math]::PI / 180
    $lat2 = $To.Latitude * [Math]::PI / 180
    $longitudeDifference = ($To.Longitude - $From.Longitude) * [Math]::PI / 180

    $y = [Math]::Sin($longitudeDifference) * [Math]::Cos($lat2)
    $x = [Math]::Cos($lat1) * [Math]::Sin($lat2) -
        [Math]::Sin($lat1) * [Math]::Cos($lat2) * [Math]::Cos($longitudeDifference)

    return ([Math]::Atan2($y, $x) * 180 / [Math]::PI + 360) % 360
}

$endpoint = "$($ApiBaseUrl.TrimEnd('/'))/api/gps/receive"

Write-Host "Sending GPS updates for device $DeviceId"
Write-Host "Endpoint: $endpoint"
Write-Host "GPX track: $GpxPath"
Write-Host "Using $Count sampled points from $($trackPoints.Count) recorded points."
Write-Host "Press Ctrl+C to stop."

for ($i = 0; $i -lt $Count; $i++) {
    $point = $route[$i]
    $nextPoint = if ($i -lt ($route.Count - 1)) { $route[$i + 1] } else { $route[$i] }
    $heading = Get-Heading -From $point -To $nextPoint
    $payload = @{
        deviceId = $DeviceId
        latitude = $point.Latitude
        longitude = $point.Longitude
        speed = $Speed
        heading = $heading
        accuracy = 5
        status = "Moving"
        signal = 90
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod `
            -Uri $endpoint `
            -Method Post `
            -ContentType "application/json" `
            -Body $payload

        $timestamp = Get-Date -Format "HH:mm:ss"
        Write-Host "[$timestamp] Point $($i + 1)/${Count}: $($point.Latitude), $($point.Longitude)" -ForegroundColor Green
    }
    catch {
        $message = $_.Exception.Message
        if ($_.ErrorDetails.Message) {
            $message = $_.ErrorDetails.Message
        }

        Write-Host "GPS update failed: $message" -ForegroundColor Red
        Write-Host "Confirm that a trip using device $DeviceId is In Progress." -ForegroundColor Yellow
        exit 1
    }

    if ($i -lt ($Count - 1)) {
        Start-Sleep -Seconds $IntervalSeconds
    }
}

Write-Host "Simulation finished after $Count updates." -ForegroundColor Cyan
