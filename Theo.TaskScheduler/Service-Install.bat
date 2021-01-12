@echo.服务启动......  
@echo off  
@sc create TheoTaskScheduler binPath= "publish\Theo.TaskScheduler.exe"
@sc description TheoTaskScheduler "TheoDemo-任务调度服务"
@net start TheoTaskScheduler  
@echo off
@echo.启动完毕！  
@pause