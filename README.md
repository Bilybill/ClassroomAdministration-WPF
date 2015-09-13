校园课务管理兼课程表软件。

###个人信息页面
用户在“个人信息”页面可以查看所有自己申请的活动。

###课程表页面
自由添加和删除课程,搭建自己的课程表。
可以选择教室查看其课程表,并和自己的课程表进行比对。
在自己和教室的课程表都为空闲的时间点,可以立即申请活动。尚未通过审核的活动将标记为“(未审核)”。
使用鼠标滚轮可以快速切换周数。

###教室列表页面
选择任意一个时间点后,可以查看此时所有教室的课程情况。

###系统消息页面
管理员可在自己的“系统消息”页面查看所有未审核的活动并进行处理。处理结果将发送到用户的系统消息页面。

###一键换肤
更换背景和所有文字的颜色,以及多个文本框的背景颜色。

---

### Rent 教室的使用
- int 		rId 
- int 		cId 
- bool 		Approved   
- RentTime 	Time 
- int 		pId 申请人
- string 	Info 基本信息

### RentTable 课程列表
- List< Rent >	Rents

### Schedule 用于课程表UI
- RentTableOwner Owner
- Grid GridScheduleHead 表头
- Grid GridSchedule 表格主体
- Label RectangleChosonClass 选定框
- WindowIndex Father 父
- Label[] head 表头里的内容
- List< TextBlock > TextBlockRents 使用TextBlock显示课程
- Rent chosenRent 当前选中的课程
- TextBlock TBHighlight 当前高亮的课程
- void SetDateClass(DateTime date, int cc) 择日期时间
- void checkoutWeek() 检查课程是否在本周
- void ChosenRentControl() 检查选定的课程

---

### RentTableOwner
- string 		name
- RentTable 	RentTable
- abstract void GetMyRentTable()

### Person : RentTableOwner
- int 		pId
- void 		DeleteFromMyRentTable(int rId) 
- void 		AddToMyRentTable(int rId)

### Administrator : Person  
- bool 		ApproveRent(Rent r)

### User : Person 
- string 	Department 专业

### Building 
- int bId 
- string Name
- List< Classroom > Classrooms 教学楼里所有的教室

### Classroom : RentTableOwner
- int cId 
- Building Building 所在的教学楼

---

### MySQL数据库

create table person
(
pId 			int not null primary key,
name 			varchar(10) not null,
password 		varchar(32),
department		varchar(20),
sex				char(1)
);

create table rent
(
rId				int(9) not null auto_increment primary key,
cId				int(6) not null,
approved		bool not null,
pId				int,
info			varchar(280),
startDate		date,
endDate			date,
cycDays			int,
startClass		int,
endClass		int
);

create table takePartIn
(
pId				int,
rId				int
);

create table SysMsg
(
sendId			int,
recvId			int,
time			date,
info			varchar(280)
);

### 数据库操作
###Get person

- Person Login(int pId, string tPassword)
- string GetName(int pId)

###Get Rent and RentTable

- Rent GetRent(int rId)
- RentTable GetPersonRentTable(int pId)
- RentTable GetClassroomRentTable(int cId)
- RentTable GetDateRentTable(DateTime date)
- List< Rent > GetUnapprovedRentTable()
- List< Rent > GetMyApplyingRents(int pId)
- bool SetRent(Rent r)
- bool ApproveRent(Rent r)
- bool DeleteRent(Rent r)

###Get SysMsg

- List< SysMsg > GetPersonSysMsgList(int pId)
- bool SendSysMsg(SysMsg msg)
- bool DeleteSysMsg(SysMsg msg)

###Get takepartin

- List< int > GetPIdList(int rId)
- bool AddTakepartin(int pId, int rId)
- bool DeleteTakepartin(int pId, int rId)


### 存于客户端的静态数据
- Building.GetBuilding()
- Building.GetClassroom()