# To use this, 
# pip3 install requests flask flas_sqlalchemy eventlet flask_socketio

#Source Code: Dr. Schaub's service3.py from the class examples


# Comment header for all methods (parameters, return type)

#TODO List (Future Deliverables): 
# Better Feedback
# Better Validation 
# Admin operation validation 
# Use python decorators

#For User Table...
    #notify that only Visa, MasterCard, and Discover and valid credit cards
    #have client code store cvv and expiration date

#For Ticket Table...
    #disable the to_dict_all() method later for security reasons

#For Ticket Table...
    #have client notify user of 10 seat booking cap


from flask import Flask, request, abort, jsonify, Response
from flask.helpers import make_response
from werkzeug.exceptions import HTTPException
from datetime import datetime, timedelta
import json, time, uuid, hashlib
from flask_sqlalchemy import SQLAlchemy
from flask_socketio import SocketIO
from flask_socketio import send, emit
from threading import Lock

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///ticketing_app_table.sqlite3'
app.config['SECRET_KEY'] = 'secret!'
db = SQLAlchemy(app)
socketio = SocketIO(app, logger=True, engineio_logger=True)
salt = "mysaltsecret"
lock = Lock()

'''
***************************
BEGIN TABLE 1
'''

# Model class 'events'
# Sql Table for Events
class Event(db.Model):
    __tablename__= 'events'
    id = db.Column(db.Integer, primary_key=True)
    title = db.Column(db.String(40), nullable=False)
    description = db.Column(db.String(160), nullable=False)
    priceperseat = db.Column(db.Float, nullable=False)
    durationhours = db.Column(db.Float, nullable=False)
    dateandtime = db.Column(db.DateTime, nullable=False)

    seatingchart = db.relationship("Seating")

    def to_dict(self):        
        # See https://stackoverflow.com/questions/7102754/jsonify-a-sqlalchemy-result-set-in-flask for a general solution
        return { 'id': self.id, 'title': self.title, 'description': self.description, 'priceperseat': self.priceperseat, 'durationhours': self.durationhours, 'dateandtime': self.dateandtime}

'''
END TABLE 1
***************************
'''




'''
***************************
BEGIN TABLE 2
'''

# Model class 'seating'
# Sql Table for Seats
class Seating(db.Model):
    __tablename__= 'seating'
    eventlinkid = db.Column(db.Integer, db.ForeignKey('events.id'), primary_key=True)
    rowid = db.Column(db.String(1), primary_key=True)
    colid = db.Column(db.Integer, primary_key=True)
    status_held = db.Column(db.Boolean, nullable=False)
    userid_heldby = db.Column(db.String(36), nullable=True)
    expiration_hold_date = db.Column(db.DateTime, nullable=True)
    status_reserved = db.Column(db.Boolean, nullable=False)
    userid_reservation = db.Column(db.String(36), nullable=True)

    def to_dict(self):
        return {'eventlinkid': self.eventlinkid, 'rowid': self.rowid, 'colid': self.colid,'status_held':self.status_held, 'userid_heldby':self.userid_heldby,'expiration_hold_date':self.expiration_hold_date,'status_reserved':self.status_reserved,'userid_reservation': self.userid_reservation}


'''
END TABLE 2
***************************
'''



'''
***************************
BEGIN TABLE 3
'''

# Model class 'users'
# Sql Table for Users
class User(db.Model): 
    __tablename__ = 'users'
    id = db.Column(db.String(36), primary_key=True) #STORE AS UUID
    email = db.Column(db.String(64), nullable=False)
    password = db.Column(db.String(64), nullable=False)
    creditcard_number = db.Column(db.String(20), nullable=True)
    creditcard_expdate = db.Column(db.DateTime, nullable=True)
    creditcard_cvv = db.Column(db.Integer(), nullable=True)

    def to_dict_secure(self):
        return {'id': self.id, 'email': self.email}

    def to_dict_all(self):
        return {'id': self.id, 'email': self.email, 'password': self.password, 'creditcard_number': self.creditcard_number, 'creditcard_expdate': self.creditcard_expdate, 'creditcard_cvv': self.creditcard_cvv}
'''
END TABLE 3
***************************
'''


'''
***************************
BEGIN TABLE 4
'''

# Model class 'tickets'
# Sql Table for Tickets
class Ticket(db.Model): 
    __tablename__ = 'tickets'
    id = db.Column(db.String(36), primary_key=True) #STORE AS UUID
    userlinkid = db.Column(db.Integer(), db.ForeignKey('users.id'), nullable=False)
    eventlinkid = db.Column(db.Integer(), db.ForeignKey('events.id'), nullable=False)
    eventtitle = db.Column(db.String(40), nullable=False)
    seats_reserved = db.Column(db.String(60),nullable=False) #CAN ONLY BOOK A MAX OF 10 SEATS
    creditcard_used = db.Column(db.String(20),nullable=False)
    total_price = db.Column(db.Float(),nullable=False)

    def to_dict(self):
        return {"id":self.id,"userlinkid": self.userlinkid,"eventlinkid":self.eventlinkid,"eventtitle":self.eventtitle,"seats_reserved":self.seats_reserved,"total_price":self.total_price,"creditcard_used":self.creditcard_used}
'''
END TABLE 4
***************************
'''

'''
***************************
BEGIN TABLE 5
'''

# Model class 'tickets'
# Sql Table for Tickets
class Session(db.Model): 
    __tablename__ = 'loginsession'
    id = db.Column(db.String(36), primary_key=True) #STORE AS UUID
    userlinkid= db.Column(db.String(36),db.ForeignKey('users.id'),nullable=False)
    creation_date = db.Column(db.DateTime, nullable=False)
    expiration_date = db.Column(db.DateTime, nullable=False)

    def to_dict(self):
        return {'id': self.id,'userlinkid': self.userlinkid,'creation_date': self.creation_date,'expiration_date': self.expiration_date}
'''
END TABLE 5
***************************
'''

# Create tables from model classes
db.create_all()

# ----------------------------------------
# Central error handler. 
# Converts abort() and unhandled application exceptions to JSON responses, and logs them
# ----------------------------------------
@app.errorhandler(Exception)
def handle_error(e):
    
    if isinstance(e, HTTPException):
        return jsonify(error=str(e)), e.code

    # An unhandled exception was thrown ... log the details
    data = request.json if request.json else request.data
    app.logger.error('%s %s\nRequest body: %s\n', request.method, request.full_path, data, exc_info=e)

    return jsonify(error=str(e)), 500

@app.before_request
def start_request():
    begin_cleanup()
    app.logger.warning('%s %s - Starting request processing', request.method, request.full_path)


'''
LOGIN DECORATOR 
'''
def validate_session(login_session:str): 
    user_session = Session.query.filter_by(id=login_session).first()
    if user_session is None:
        abort(401, description=f'Unauthorized access')
    return user_session

'''
CLEAN-UP TIMERS
'''
def begin_cleanup():
    cleanup_sessions()
    reset_heldseats()


'''
HELD SEATS CHECK
'''
def reset_heldseats():
    my_time = datetime.now()
    heldseats_list = Seating.query.all()
    for seat in heldseats_list:
        if (seat.expiration_hold_date!=None and my_time > seat.expiration_hold_date):
            seat.expiration_hold_date = None
            seat.status_held = False
            seat.userid_heldby = None
    db.session.commit()

'''
SESSION CHECK
'''
def cleanup_sessions():
    my_time = datetime.now()
    session_list = Session.query.all()
    for session_item in session_list:
        if (my_time > session_item.expiration_date):
            db.session.delete(session_item)
    db.session.commit()


'''
***************************
BEGIN TABLE 1 API CALLS (EVENT)
'''

# Validation method
# @param: event id
# @detail: validates that requested event exists
# @return: event record object
def validate_event(event_id: int): 
    event = Event.query.filter_by(id=event_id).first()
    if event is None:
        abort(404, description=f'No event with id {event_id} exists.')
    return event
        
# HTTP Request
# @return list of events
@app.route("/events", methods=['GET'])
def get_eventlist():
    events = Event.query.all()
    return jsonify([event.to_dict() for event in events])

# HTTP Request
# @param: event id
# @return single event in the db
@app.route("/events/<int:event_id>", methods=['GET'])
def get_eventdetail(event_id: int):
    ev = validate_event(event_id)
    if ev:
        return jsonify(ev.to_dict())

# HTTP Request (ADMIN)
# @param: json {title, description, price per seat, duration in hours, date and time of event}
# @detail: creates requested event record; price and duration must be positive; does NOT check for duplicates
# @detail: uploads event record in db
# @return: returns event record in json{}
@app.route("/events", methods=['POST'])
def create_event():
    event_dict = request.get_json()
    try:
        title_in = event_dict['title']
        description_in = event_dict['description']
        pricequote_in = float(event_dict['priceperseat'])
        durationh_in = float(event_dict['durationhours'])
        dateandtime_in = datetime.strptime(event_dict['dateandtime'],"%m/%d/%Y, %H:%M:%S")
        if pricequote_in < 0:
            abort(400, description=f'Price may not be negative.')
        elif durationh_in <= 0:
            abort(400, description=f'Duration may not be 0 or negative.')
        
    except Exception as e:
        abort(400, description=f'Invalid request: {e}')

    ev = Event(title=title_in, description=description_in, priceperseat=pricequote_in,durationhours=durationh_in,dateandtime=dateandtime_in)
    db.session.add(ev)
    db.session.commit()

    return jsonify(ev.to_dict())

# HTTP Request (ADMIN)
# @param: id of event
# @detail: validates if event exists, then removes record from events table
# @return: confirmation message
@app.route("/events/<int:event_id>", methods=['DELETE'])
def delete_event(event_id:int): 
    ev = validate_event(event_id)
    if ev:
        db.session.delete(ev)
        db.session.commit()
    return f'Sucessfully deleted event with id of {event_id}'

'''
END TABLE 1 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 2 API CALLS (SEATING)
'''

# Validation method
# @param: seating chart in string format "A1, A2, A3, ..." and id of event 
# @detail: checks for proper formatting and creates list of seat records
# @note: ONLY to be used for creation of new seating charts for events
# @return: list of seat record objects
def validate_seatingchart_format(seatingchart:str, event_id:int):
    seatchart_list = [s.strip() for s in seatingchart.split(",")]
    seatingchart_return = []

    for seat in seatchart_list:
        if len(seat) != 2:
            abort(400, description=f'Seat specification must be 2 characters long.')
        elif (seat[0].isalpha() is False or seat[0].islower()):
            abort(400, description=f'Row indexes must be capital letters only.')
        elif (seat[1].isnumeric() is False or int(seat[1]) <= 0):
            abort(400, description=f'Column indexes must be whole numbers and non-zero.')
        
    for seat in seatchart_list:
        seatingchart_return.append(Seating(eventlinkid=event_id,rowid=seat[0],colid=seat[1]))

    if not seatingchart_return:
        abort(400, description=f'Input Seats are empty. Please make requests with non-empty seats.')

    return seatingchart_return


# Validation method
# @param: seating chart in string format "A1, A2, A3, ..." and id of event 
# @detail: checks for proper formatting and creates list of seat records
# @return: list of seat record objects
def validate_seats(seatingchart:str, event_id:int):

    seatchart_list = [s.strip() for s in seatingchart.split(",")]
    seatingchart_return = []
        
    for seat in seatchart_list:
        seat_db = Seating.query.filter_by(eventlinkid=event_id,rowid=seat[0],colid=int(seat[1])).first()
        if seat_db is None:
            abort(400, description=f'Seat {seat} for event id {event_id} does not exist')
    
    for seat in seatchart_list:
        seatingchart_return.append(Seating(eventlinkid=event_id,rowid=seat[0],colid=int(seat[1])))

    return seatingchart_return

# HTTP Request
# @param: id of event
# @return: list of seats for that event in json{}
@app.route("/events/<int:event_id>/seating", methods=['GET'])
def get_eventseating(event_id: int):
    seats = Seating.query.filter_by(eventlinkid=event_id).all()
    return jsonify([seat.to_dict() for seat in seats])


# HTTP Request (ADMIN)
# @param: id of event, dict {"seatingchart": "A1, A2, A3, A4, ..."}
# @detail: uploads requested list of seat records in db if valid
# @return: Confirmation message
@app.route("/events/<int:event_id>/seating", methods=['POST'])
def create_eventseatingchart(event_id: int):
    seating_chart_dict = request.get_json()
    ev = validate_event(event_id)
    try:
        seatchart_in = seating_chart_dict["seating_chart"]
        seatchart_list = validate_seatingchart_format(seatchart_in,event_id)
        
        for seat in seatchart_list:
            seat.status_held = False
            seat.status_reserved = False
            db.session.add(seat)
        
        db.session.commit()

    except Exception as e: 
        abort(400, description=f'Invalid request: {e}')

    return f'Successfully creating seating chart for event {event_id} with seats {seatchart_in}'

# HTTP Request (USER-SPECIFIC)
# @param: (str) login session and (str) comma deliminated list of seats
# @detail: if login session is valid and seats aren't held or reserved, place a hold on those seats
# @return: (str) Confirmation message
@app.route("/events/<int:event_id>/seating", methods=['PUT'])
def hold_selectedseats(event_id: int):
    selected_seats_dict = request.get_json()
    loginsession_in = str(request.cookies.get('loginsession'))
    ev = validate_event(event_id)
    myloginsession = validate_session(loginsession_in)
    expiration_date_in = datetime.now() + timedelta(minutes=10)
    try:
        selected_seats_in = selected_seats_dict["selectedseats"]

        seat_list = validate_seatingchart_format(selected_seats_in, event_id)

        with lock:
            validate_seats_hold_availability(seat_list)

            for seat in seat_list:
                update_seat = Seating.query.filter_by(eventlinkid=seat.eventlinkid,rowid=seat.rowid, colid=seat.colid).first()
                update_seat.userid_heldby = myloginsession.userlinkid
                update_seat.status_held = True
                update_seat.expiration_hold_date = expiration_date_in

            db.session.commit()

        totalprice_in  = calculate_ticketprice(len(seat_list),ev)
        return str(totalprice_in)

    except Exception as e: 
        abort(400, description=f'Invalid request: {e}')



'''
END TABLE 2 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 3 API CALLS
'''

# Validation method
# @param: id of user
# @detail: checks if requested user exists in db
# @return: user record object
def validate_user(user_id: str): 
    user = User.query.filter_by(id=user_id).first()
    if user is None:
        abort(404, description=f'No user with id {user_id} exists')
    return user

#Encryption method
# @param: original unencoded password 
# @return: encrypted password
def hash_password(password_in: str):
    global salt
    return hashlib.sha256(bytes(salt+password_in,"ascii")).hexdigest()

# HTTP Request (ADMIN ONLY)
# @return: list of users in the db
@app.route("/users", methods=['GET'])
def get_allusers():
    users = User.query.all()
    return jsonify([user.to_dict_secure() for user in users])

# HTTP Request (USER-SPECIFIC)
# @param: (str) username and (str) password
# @description: login method
# @return: temporary user-specific session token
@app.route("/users/login", methods=['POST'])
def create_userloginsession():
    login_dict = request.get_json()
    print("Login attempt with data: " + json.dumps(login_dict))
    try:
        email_in = login_dict["email"]
        password_in = hash_password(login_dict["password"])
        login_user = User.query.filter_by(email=email_in,password=password_in).first()

        if (login_user is None): 
            abort(404, description=f'Username or password does not match.')

        # If pass, then create login session
        id_in = str(uuid.uuid4())
        date_creation_in = datetime.now()
        expiration_date_in = datetime.now() + timedelta(hours=1)
        my_session = Session(id=id_in,userlinkid=login_user.id,creation_date=date_creation_in,expiration_date=expiration_date_in)
        db.session.add(my_session)
        db.session.commit()

        #resp = make_response()
        #resp.set_cookie('loginsession',id_in)
        return id_in

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')

# HTTP Request (USER-SPECIFIC)
# @param: (str) login session id
# @return: detailed info of user, including sensitive info
@app.route("/users/myprofile", methods=['GET'])
def get_userinfo():
    loginsession_in = request.cookies.get('loginsession')
    myloginsession = validate_session(loginsession_in)

    my_user = User.query.filter_by(id=myloginsession.userlinkid).first()
    return jsonify(my_user.to_dict_all())


# HTTP Request (ADMIN)
# @param: dict {email, password, creditcard number, credit card expiration date, credit card cvv}
# @detail: creates a User object record and inserts it into the db
# @return: user object record in json{}
@app.route("/users", methods=['POST'])
def create_user():
    user_dict = request.get_json()
    try: 
        id_in = str(uuid.uuid4())
        email_in = user_dict["email"]
        password_in = hash_password(user_dict["password"])
        creditcard_num_in = None
        creditcard_exp_in = None
        creditcard_cvv_in = None
        try: 
            creditcard_num_in = user_dict["creditcard_num"]
            creditcard_exp_in = datetime.strptime(user_dict["creditcard_exp"],"%m/%d/%Y, %H:%M:%S")
            creditcard_cvv_in = user_dict["creditcard_cvv"]
        except:
            print("@ '/users' POST request: no/invalid credit card info.")

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')


    user = User(id=id_in,email=email_in,password=password_in,creditcard_number=creditcard_num_in,creditcard_expdate=creditcard_exp_in,creditcard_cvv=creditcard_cvv_in)
    db.session.add(user)
    db.session.commit()

    return jsonify(user.to_dict_all())


# HTTP Request (ADMIN)
# @param: id of user
# @detail: deletes the User record if the user exists
# @return: confirmation of message
@app.route("/users/<user_id>", methods=['DELETE'])
def delete_user(user_id:str):
    user_in = validate_user(user_id)

    if user_in:
        db.session.delete(user_in)
        db.session.commit()
        return f'Successfully deleted user of id {user_id}'

'''
END TABLE 3 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 4 API CALLS
'''

# TICKET VALIDATION 
def validate_ticket(ticket_id: str):
    ticket = Ticket.query.filter_by(id=ticket_id).first()
    if ticket is None:
        abort(404, description=f'No ticket with id {ticket_id} exists')
    return ticket

# HTTP REQUEST (ADMIN)
@app.route("/tickets")
def get_alltickets(): 
    tickets = Ticket.query.all()
    return jsonify([ticket.to_dict() for ticket in tickets])

# HTTP REQUEST (USER-SPECIFIC)
# @param: (str) login session
# @description: Get user list of tickets
@app.route("/users/myprofile/tickets", methods=['GET'])
def get_usertickets():
    loginsession_in = request.cookies.get('loginsession')
    myloginsession = validate_session(loginsession_in)

    tickets = Ticket.query.filter_by(userlinkid=myloginsession.userlinkid)
    return jsonify([ticket.to_dict() for ticket in tickets])


# HTTP REQUEST (USER-SPECIFIC)
# Get user single ticket
@app.route("/users/myprofile/tickets/<ticket_id>", methods=['GET'])
def get_userticket(ticket_id: str):
    loginsession_in = request.cookies.get('loginsession')
    myloginsession = validate_session(loginsession_in)

    ticket_in = validate_ticket(ticket_id)

    return jsonify(ticket_in.to_dict())

# HTTP REQUEST (USER-SPECIFIC)
# @param: (str) user session and (str) ticket id
# @detail: delete user specified ticket 
@app.route("/users/myprofile/tickets/<ticket_id>", methods=['DELETE'])
def delete_userticket(ticket_id: str):
    loginsession_in = request.cookies.get('loginsession')
    myloginsession = validate_session(loginsession_in)
    ticket_in = validate_ticket(ticket_id)

    if ticket_in: 
        db.sesssion.delete(ticket_in)
        db.session.commit()
        return f'Successfully deleted ticket with id of {ticket_id}'
    

#HTTP REQUEST (USER-SPECIFIC)
# @param: 
@app.route("/users/myprofile/tickets", methods=['POST'])
def create_userticket():
    loginsession_in = request.cookies.get('loginsession')
    myloginsession = validate_session(loginsession_in)
    userticket_dict = request.get_json()
    id_in = str(uuid.uuid4())
    try: 
        eventid_in = userticket_dict["eventid"]
        seatsreq_in = userticket_dict["seats"]
        creditcard_in = userticket_dict["creditcardnum"]

        userid_in = validate_user(myloginsession.userlinkid)
        event_in = validate_event(eventid_in)
        validate_seatingchart_format(seatsreq_in,eventid_in)
        seatlist_in = validate_seats(seatsreq_in,eventid_in)

        with lock:
            validate_seats_reserve_availability(seatlist_in,myloginsession.userlinkid)

            totalprice_in  = calculate_ticketprice(len(seatlist_in),event_in)
            t = Ticket(id=id_in,userlinkid=userid_in.id,eventlinkid=eventid_in,eventtitle=event_in.title,seats_reserved=seatsreq_in,creditcard_used=creditcard_in,total_price=totalprice_in)
            db.session.add(t)

            for seat in seatlist_in:
                update_seat = Seating.query.filter_by(eventlinkid=seat.eventlinkid,rowid=seat.rowid, colid=seat.colid).first()
                update_seat.status_reserved = True
                update_seat.userid_reservation = myloginsession.userlinkid
            
            db.session.commit()

        return jsonify(t.to_dict())

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')
    
    #return f'Suceessfully reserved ticket with id of {id_in}'

def validate_seats_hold_availability(chosenseats: list):
    isInvalid = False
    invalidSeats = ""
    for seat in chosenseats:
        seatstatus = Seating.query.filter_by(eventlinkid=seat.eventlinkid,rowid=seat.rowid, colid=seat.colid).first()
        if (seatstatus.status_held == True) and (seatstatus.status_reserved == True):
            isInvalid = True
            invalidSeats+= str(seat.rowid) + str(seat.colid) + ","
    
    if isInvalid:
        abort(400, description=f'The seats that you selected, {invalidSeats} are not available.')

    return chosenseats

def validate_seats_reserve_availability(chosenseats: list, userid: str):
    isInvalid = False
    invalidSeats = ""
    for seat in chosenseats:
        seatstatus = Seating.query.filter_by(eventlinkid=seat.eventlinkid,rowid=seat.rowid, colid=seat.colid).first()
        if (seatstatus.status_reserved==True) or (seatstatus.userid_heldby != userid):
            isInvalid = True
            invalidSeats+= str(seat.rowid) + str(seat.colid) + ","
    
    if isInvalid:
        abort(400, description=f'The seats that you selected, {invalidSeats} are not available.')

    return chosenseats
    


def calculate_ticketprice(numseats: int, event: Event):
    if (numseats <= 0 ):
        abort(400, description=f'Cannot book 0 tickets. Must book at least 1 ticket.')
    priceperseat = event.priceperseat

    return numseats * priceperseat

'''
END TABLE 4 API CALLS
***************************
'''

'''
BEGIN TABLE 5 API CALLS
'''

# HTTP REQUEST (ADMIN)
@app.route("/sessions", methods=['GET'])
def get_allsessions():
    sessions = Session.query.all()
    return jsonify([session.to_dict() for session in sessions])

'''
END TABLE 5 API CALLS
'''


'''
TEST CODE (To Be Deleted)
'''

@app.route("/setcookie", methods=['GET'])
def setcookie():
    value = "cookievalue"
    resp = make_response()
    resp.set_cookie('key',value)
    return resp

@app.route("/getcookie", methods=['GET'])
def getcookie():
    value = str(request.cookies.get('key'))
    print('@Debug!: value is ' + value)
    return "All good."


'''
***************************
BEGIN WEB SOCKET API CALLS 
NOT USED
'''

@socketio.on('connect')
def on_connect(auth):
    print('Client connected')
    emit('jtan716', {'data': 'You are now connected to the jtan ticketing app sever.'})

@socketio.on('disconnect')
def on_disconnect():
    print('Client disconnected')

@socketio.on('greeting')
def on_greeting(data):
    print(f'Greeting: {data}')

'''
END WEB SOCKET API CALLS 
***************************
'''

# Startup Server 
if __name__ == '__main__':
    app.run(host="0.0.0.0", debug=True) # for pretty print
    #socketio.run(app, use_reloader=True) # for client functionality
