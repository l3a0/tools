# Sign in to your Microsoft Azure account in resource management mode and select
# your subscription as follows:
Connect-AzureRmAccount
Select-AzureRmSubscription -SubscriptionName 'CRM-DevTest-BTS-Dev'

# Set your resource group name and VM name as follows:
$rgName = 'crmbts-devmachines-crmbts-gramsay-586704'
$vmName = 'crmbts-gramsay'

# Obtain a reference to your VM as follows:
$vm = Get-AzureRmVM -ResourceGroupName $rgName -Name $vmName

# Stop the VM before resizing the disk as follows:
Stop-AzureRmVM -ResourceGroupName $rgName -Name $vmName

# Obtain a reference to the managed OS disk. Set the size of the managed OS disk
# to the desired value and update the Disk as follows:
# The new size should be greater than the existing disk size. The maximum
# allowed is 2048 GB for OS disks. (It is possible to expand the VHD blob beyond
# that size, but the OS will only be able to work with the first 2048 GB of space.)
$disk= Get-AzureRmDisk -ResourceGroupName $rgName -DiskName $vm.StorageProfile.OsDisk.Name
$disk.DiskSizeGB = 1023
Update-AzureRmDisk -ResourceGroupName $rgName -Disk $disk -DiskName $disk.Name

# Updating the VM may take a few seconds. Once the command finishes executing,
# restart the VM as follows:
Start-AzureRmVM -ResourceGroupName $rgName -Name $vmName

# Expand the volume within the OS
# Once you have expanded the disk for the VM, you need to go into the OS and
# expand the volume to encompass the new space. There are several methods for
# expanding a partition. This section covers connecting the VM using an RDP
# connection to expand the partition using DiskPart.

# Open an RDP connection to your VM.

# Open a command prompt and type diskpart.

# At the DISKPART prompt, type list volume. Make note of the volume you want to
# extend.

# At the DISKPART prompt, type select volume <volumenumber>. This selects the
# volume volumenumber that you want to extend into contiguous, empty space on
# the same disk.

# At the DISKPART prompt, type extend [size=<size>]. This extends the selected
# volume by size in megabytes (MB).