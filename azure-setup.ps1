$ErrorActionPreference = "Stop"

$subscriptionId = $(az account show --query id --output tsv)

if(!$subscriptionId) {
    Write-Host "You are not logged in to any subscription. Please run 'az login' and try again."
    exit
}

Write-Host "Current subscription: $(az account show --output json | jq .name -r)"
$continue = Read-Host "Continue with this subscription? (y/n)"
if ($continue -notmatch '^(y|Y)$') {
    exit
}

Select-AzSubscription -SubscriptionId $subscriptionId | Out-Null

function CreateRole {
    Write-Host "Creating role 'DevOpsCandidate'..."

    $roleNames = $(az role definition list --name "DevOpsCandidate" --query "[].name" --output tsv --only-show-errors)
    if ($roleNames) {
        Write-Host "Role already exists. Skipping creation."
        return
    }

    $roleJson = @{
        "Name" = "DevOpsCandidate"
        "Description" = "Role for the DevOps candidate."
        "Actions" = @("*")
        "AssignableScopes" = @("/subscriptions/$subscriptionId")
        "NotActions" = @(
            "Microsoft.Authorization/elevateAccess/Action"
            "Microsoft.Blueprint/blueprintAssignments/write"
            "Microsoft.Blueprint/blueprintAssignments/delete"
            "Microsoft.Compute/galleries/share/action"
            "Microsoft.Purview/consents/write"
            "Microsoft.Purview/consents/delete"
            "Microsoft.Resources/deploymentStacks/manageDenySetting/action"
            "Microsoft.Subscription/cancel/action"
            "Microsoft.Subscription/enable/action"
        )
    } | ConvertTo-Json -Depth 10

    $o = $($roleJson | az role definition create --role-definition "@-" --only-show-errors)
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create role."
        exit
    }
    sleep 5
    Write-Host "Role created successfully."
}

function CreateServicePrincipal {
    $spName = "rxnt-devops-candidate"
    Write-Host "Creating Service Principal '$spName'..."

    $sp = Get-AzADServicePrincipal -DisplayName $spName
    if (!$sp) {
        $o = $(az ad sp create-for-rbac --name $spName --role="DevOpsCandidate" --scopes="/subscriptions/$subscriptionId" --only-show-errors)
        sleep 5
        Write-Host "Service Principal created."
    } else {
        Write-Host "Service Principal '$spName' already exists. Skipping creation."
    }

    $app = Get-AzADApplication -DisplayName $spName
    if (!$app) {
        Write-Host "Waiting the application to be created, please be patient..."
        sleep 10
        $app = Get-AzADApplication -DisplayName $spName
        if(!$app) {
            Write-Host "Failed to create Application '$spName'."
            exit
        }
    }

    Write-Host "Generating client secret..."
    $tomorrow = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $output = $(az ad app credential reset --id $app.AppId --append --display-name "$spName" --end-date "$tomorrow" --only-show-errors)

    $client_secret = $output | jq .password
    $client_id = $output | jq .appId
    $tenant_id = $output | jq .tenant

    Write-Host ""
    Write-Host "Please test the credentials by running the following commands:"
    Write-Host "az logout"
    Write-Host "az account clear"
    Write-Host "az login --service-principal -u $client_id -p $client_secret --tenant $tenant_id"
    Write-Host "az account list"
}

Write-Host ""
CreateRole
Write-Host ""
CreateServicePrincipal
Write-Host ""