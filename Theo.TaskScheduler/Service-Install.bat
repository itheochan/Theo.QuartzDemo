@echo.��������......  
@echo off  
@sc create TheoTaskScheduler binPath= "publish\Theo.TaskScheduler.exe"
@sc description TheoTaskScheduler "TheoDemo-������ȷ���"
@net start TheoTaskScheduler  
@echo off
@echo.������ϣ�  
@pause