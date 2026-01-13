# Script de Migração para Unity 6
# Substitui APIs obsoletas do Unity pelos novos métodos
#
# Como usar:
# 1. Abra o PowerShell
# 2. Navegue até a pasta do projeto
# 3. Execute: .\migrate-to-unity6.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Migração para Unity 6 - TimeCrax" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptsPath = ".\Assets\Scripts"
$totalReplacements = 0
$filesModified = 0

if (-not (Test-Path $scriptsPath)) {
    Write-Host "ERRO: Pasta Assets\Scripts não encontrada!" -ForegroundColor Red
    Write-Host "Execute este script na raiz do projeto Unity." -ForegroundColor Yellow
    exit 1
}

# Criar backup
$backupPath = ".\Assets\Scripts_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Criando backup em: $backupPath" -ForegroundColor Yellow
Copy-Item -Path $scriptsPath -Destination $backupPath -Recurse
Write-Host "Backup criado com sucesso!" -ForegroundColor Green
Write-Host ""

# Processar arquivos
$csFiles = Get-ChildItem -Path $scriptsPath -Filter "*.cs" -Recurse

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileReplacements = 0

    # Substituir FindObjectsOfType<T>() -> FindObjectsByType<T>(FindObjectsSortMode.None)
    $pattern1 = 'FindObjectsOfType<([^>]+)>\s*\(\s*\)'
    $replacement1 = 'FindObjectsByType<$1>(FindObjectsSortMode.None)'

    $matches1 = [regex]::Matches($content, $pattern1)
    if ($matches1.Count -gt 0) {
        $content = [regex]::Replace($content, $pattern1, $replacement1)
        $fileReplacements += $matches1.Count
        Write-Host "  $($file.Name): $($matches1.Count)x FindObjectsOfType -> FindObjectsByType" -ForegroundColor Gray
    }

    # Substituir FindObjectOfType<T>() -> FindFirstObjectByType<T>()
    $pattern2 = 'FindObjectOfType<([^>]+)>\s*\(\s*\)'
    $replacement2 = 'FindFirstObjectByType<$1>()'

    $matches2 = [regex]::Matches($content, $pattern2)
    if ($matches2.Count -gt 0) {
        $content = [regex]::Replace($content, $pattern2, $replacement2)
        $fileReplacements += $matches2.Count
        Write-Host "  $($file.Name): $($matches2.Count)x FindObjectOfType -> FindFirstObjectByType" -ForegroundColor Gray
    }

    # Salvar se houve mudanças
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $filesModified++
        $totalReplacements += $fileReplacements
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  MIGRAÇÃO CONCLUÍDA!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Arquivos modificados: $filesModified" -ForegroundColor White
Write-Host "  Total de substituições: $totalReplacements" -ForegroundColor White
Write-Host ""
Write-Host "  Backup salvo em: $backupPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "  1. Abra o projeto no Unity 6" -ForegroundColor White
Write-Host "  2. Atualize o Photon PUN para v2.50+" -ForegroundColor White
Write-Host "  3. Teste a conexão multiplayer" -ForegroundColor White
Write-Host ""
