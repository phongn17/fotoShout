// Events View Model

function EventsViewModel(baseUri) {
    var self = this;
    self.user = ko.observable(null);
    self.events = ko.observableArray();
    self.error = ko.observable(null);
    self.msg = ko.observable(null);

    $.ajax({
        type: "GET",
        url: "/api/fs1/Authorization",
        dataType: "json",
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.user(data);
        $(".username-link").append("<img class='userimage' src='/Images/User.jpg' />").append("<span class='username'>" + data.AccountName + ' > ' + data.FirstName + ' ' + data.LastName + "</span>");
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    $.ajax({
        type: "GET",
        url: baseUri,
        dataType: "json",
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.events(data);
        $("#ev-editor").load('/api/EventsTest/CreateEvent', function (response, textStatus, jqXHR) {
            if (jqXHR.status != 200)
                setError(self, jqXHR, textStatus, response);
            else
                initializeEvent("Add");
        });
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    self.submitEvent = function (formElm) {
        $(formElm).validate();
        if ($(formElm).valid()) {
            var ev = $(formElm).serialize();
            var btn = $("#ev-submit");
            if (btn.val() == "Add")
                $.ajax({
                    type: "POST",
                    url: baseUri, 
                    data: ev, 
                    dataType: "json",
                    beforeSend: function (jqXHR) {
                        addAuthorizationKey(jqXHR);
                    }
                })
                .done(function (ev) {
                    // add the new event to the view-model
                    self.events.push(ev);
                    setMessage(self, 'Added the ' + ev.EventName + ' event.')
                })
                .fail(function (jqXHR, textStatus) {
                    setError(self, jqXHR, textStatus);
                });
            else 
                $.ajax({
                    type: "PUT",
                    url: baseUri + '/' + $("#EventId").val(),
                    data: ev,
                    beforeSend: function (jqXHR) {
                        addAuthorizationKey(jqXHR);
                    }
                })
                .done(function (ev) {
                    $.ajax({
                        type: "GET",
                        url: baseUri,
                        dataType: "json",
                        beforeSend: function (jqXHR) {
                            addAuthorizationKey(jqXHR);
                        }
                    })
                    .done(function (data) {
                        self.events(data);
                        setMessage(self, 'Updated the ' + $("#EventName").val() + ' event.')
                    })
                    .fail();
                })
                .fail(function (jqXHR, textStatus) {
                    setError(self, jqXHR, textStatus);
                });
        }
    }

    self.details = function (ev) {
        $("#ev-editor").load('/api/EventsTest/EditEvent/' + ev.EventId, function (response, textStatus, jqXHR) {
            if (jqXHR.status != 200)
                setError(self, jqXHR, textStatus, response);
            else {
                initializeEvent("Update");
            }
        });
    }

    self.delete = function (ev) {
        // First remove from the server, then remove from the model-view
        $.ajax({
            type: "DELETE",
            url: baseUri + '/' + ev.EventId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function (ev) {
            self.events.remove(ev);
            setMessage(self, 'Deleted the ' + ev.EventName + ' event.')
            $.ajax({
                type: "GET",
                url: baseUri, 
                dataType: "json",
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (data) {
                self.events(data);
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.photos = function (ev) {
        location = "/api/EventsTest/AnnotatePhotos/" + ev.EventId;
    }

    self.processedPhotos = function (ev) {
        location = "/api/EventsTest/AnnotatePhotos/" + ev.EventId + "?reAnnotate=true";
    }

    self.guests = function (ev) {
        location = "/api/EventsTest/SelectGuests/" + ev.EventId;
    }

    self.openDialog = function (dlg, jqXHR, textStatus, response) {
        if (jqXHR.status == 200)
            $('#dlg-guests').dialog('open');
        else 
            setError(self, jqXHR, textStatus, response);
    }
}

function EventPhotosViewModel(eventId, reAnnotated) {
    var self = this;
    self.photos = ko.observableArray();
    self.rating = ko.observable(null);
    self.reAnnotated = ko.observable((reAnnotated == "True") ? true : false);
    self.photoGuests = ko.observableArray(null);
    self.error = ko.observable(null);
    self.msg = ko.observable(null);
    self.selected = ko.observable(null);
    self.event = ko.observable(null);
    self.user = ko.observable(null);

    $.ajax({
        type: "GET",
        url: "/api/fs1/Authorization",
        dataType: "json",
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.user(data);
        $(".username-link").append("<img class='userimage' src='/Images/User.jpg' />").append("<span class='username'>" + data.AccountName + ' > ' + data.FirstName + ' ' + data.LastName + "</span>");
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    $.ajax({
        type: "GET",
        url: '/api/fs1/Events/' + eventId,
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.event(data);
        $.ajax({
            type: "GET",
            url: (self.reAnnotated() ? '/api/fs1/EventPhotos/SubmittedPhotos/' : '/api/fs1/EventPhotos/Photos/') + eventId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function (data) {
            self.photos(data);
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    self.select = function (photo) {
        var reAnnotate = self.reAnnotated();
        if (!reAnnotate) {
            url = "/api/fs1/EventPhotos/Select/" + eventId + "?filename=" + escape(photo.Filename);
            $.ajax({
                type: "PUT",
                url: url,
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (photo) {
                self.selectPhoto(photo);
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        }
        else
            self.selectPhoto(photo);
    }

    self.selectPhoto = function (photo) {
        self.selected(photo);
        setMessage(self, "The " + photo.Filename + " is selected.");
        $.ajax({
            type: "GET",
            url: '/api/fs1/PhotoAnnotation/' + photo.PhotoId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function (data) {
            self.rating(data.Rating);
            //for (var idx = 0; idx < data.Guests.length; idx++) {
            //    $.ajax({
            //        type: "GET",
            //        url: '/fs1/EventGuests/' + self.event().EventId + '?guestId=' + data.Guests[idx].GuestId,
            //        dataType: 'image/png',
            //        beforeSend: function (jqXHR) {
            //            addAuthorizationKey(jqXHR);
            //        }
            //    })
            //    .done(function (data) {
            //        data.Guests[idx].Signature = data;
            //    })
            //    .fail(function (jqXHR, textStatus) {
            //        data.Guests[idx].Signature = null;
            //    });
            //}
            self.photoGuests(data.Guests);
            $("#email").blur(function () {
                var email = this.value;
                if (email.length > 0) {
                    email = escape(email);
                    $.ajax({
                        type: "GET",
                        url: '/api/fs1/EventGuests/' + self.event().EventId + '?email=' + email,
                        dataType: "json",
                        beforeSend: function (jqXHR) {
                            addAuthorizationKey(jqXHR);
                        }
                    })
                    .done(function (guest) {
                        if (guest != null) {
                            $("#firstname").val(guest.FirstName);
                            $("#lastname").val(guest.LastName);
                        }
                    })
                    .fail(function (jqXHR, textStatus) {
                        setError(self, jqXHR, textStatus);
                    });
                }
            });
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.unselect = function () {
        if (self.selected() == null)
            return;

        var photo = self.selected();
        $.ajax({
            type: "PUT",
            url: "/api/fs1/EventPhotos/Unselect/" + eventId + "?filename=" + photo.Filename,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            setMessage(self, "The " + photo.Filename + " is unselected.");
            self.selected(null);
            self.photoGuests(null);
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.unclaim = function (photo) {
        $.ajax({
            type: "PUT",
            url: "/api/fs1/EventPhotos/Unclaim/" + eventId + "?filename=" + photo.Filename,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            setMessage(self, "The " + photo.Filename + " is unclaimed.");
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.reclaim = function (photo) {
        $.ajax({
            type: "PUT",
            url: "/api/fs1/EventPhotos/Reclaim/" + eventId + "?filename=" + photo.Filename,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            setMessage(self, "The " + photo.Filename + " is reclaimed.");
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.unselectAll = function () {
        $.ajax({
            type: "PUT",
            url: "/api/fs1/EventPhotos/UnselectAll/" + eventId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            self.selected(null);
            self.photoGuests(null);
            $("#photo-annotation").css("display", "none");
            setMessage(self, "Unselected all photo of the " + self.event().EventName + " event.");
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.createGuest = function (guest) {
        var inputSignature = $("#signature").get(0);
        if (inputSignature.value == '') {
            self.generateGuest();
            return;
        }
        if (!window.File || !window.FileReader || !window.FileList) {
            alert("The file apis are not fully supported in this browser.");
            self.generateGuest();
            return;
        }
        var files = inputSignature.files;
        if (files == 'undefined') {
            alert("This browser doesn't seem to support the 'files' property.");
            self.generateGuest();
            return;
        }
        var signature = files[0];
        if (signature == undefined) {
            self.generateGuest();
            var guest = { FirstName: $("#firstname").val(), LastName: $("#lastname").val(), Email: $("#email").val() }
            self.photoGuests.push(guest);
        }
        else {
            var fr = new FileReader();
            fr.onload = self.displaySignature;
            fr.readAsBinaryString(signature);
        }
    }

    self.displaySignature = function () {
        self.generateGuest(btoa(this.result));
    }

    self.generateGuest = function (signature) {
        var authorizePublish = $("#authorize-publish");
        var guest = { FirstName: $("#firstname").val(), LastName: $("#lastname").val(), Email: $("#email").val(), AuthorizePublish: authorizePublish.is(":checked"), Signature: signature == undefined ? null : signature }
        self.photoGuests.push(guest);
    }

    self.deleteGuest = function (guest) {
        self.photoGuests.remove(guest);
    }

    self.submitGuests = function (formElm) {
        if (self.selected() == null) {
            alert('You need to select a photo to add the list of guests to.');
            return;
        }
        var url = "/api/fs1/PhotoAnnotation/" + self.selected().PhotoId;
        var data = ko.toJSON({ Rating: $("#rating").val(), Guests: self.photoGuests });
        $.ajax({
            type: self.reAnnotated() ? "PUT" : "POST",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: data,
            dataType: "json",
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            $.ajax({
                type: "GET",
                url: (self.reAnnotated() ? '/api/fs1/EventPhotos/SubmittedPhotos/' : '/api/fs1/EventPhotos/Photos/') + eventId,
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (data) {
                self.photos(data);
                setMessage(self, "Submitted guests to the " + self.selected().Filename + " photo.");
                self.selected(null);
                self.photoGuests(null);
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }
}


