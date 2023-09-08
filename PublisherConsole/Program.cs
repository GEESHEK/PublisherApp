using Microsoft.EntityFrameworkCore;
using PublisherData;
using PublisherDomain;

// using (PubContext context = new PubContext())
// {
//     context.Database.EnsureCreated(); //see if the database exist
// }

PubContext _context = new PubContext();

//querying the view //////////////////////////////////////////////

GetAuthorsByArtist();

void GetAuthorsByArtist()
{
    var authorartists=_context.AuthorsByArtist.ToList();
    var oneauthorartists = _context.AuthorsByArtist.FirstOrDefault();
    var Kauthorartists = _context.AuthorsByArtist
        .Where(a=>a.Artist.StartsWith("K")).ToList();
    var debugView = _context.ChangeTracker.DebugView.ShortView;
} 

//Store procedures///////////////////////////////////////////////

void RawSqlStoredProc()
{
    var authors = _context.Authors
        .FromSqlRaw("AuthorsPublishedinYearRange {0}, {1}", 2010, 2015)
        .ToList();
}

void InterpolatedSqlStoredProc()
{
    int start = 2010;
    int end = 2015;
    var authors = _context.Authors
        .FromSqlInterpolated($"AuthorsPublishedinYearRange {start}, {end}")
        .ToList();
}

//RAW SQL/////////////////////////////////////////////////////////////////////////////////////////

void SimpleRawSQL()
{
    var authors = _context.Authors.FromSqlRaw("select * from authors").ToList();
}

//ConcatenatedRawSql_Unsafe(); //There is no safe way with concatentation!
void ConcatenatedRawSql_Unsafe()
{
    var lastnameStart = "L";
    var authors = _context.Authors
        .FromSqlRaw("SELECT * FROM authors WHERE lastname LIKE '" + lastnameStart + "%'")
        .OrderBy(a => a.LastName).TagWith("Concatenated_Unsafe").ToList();
}

//FormattedRawSql_Unsafe();
void FormattedRawSql_Unsafe()
{
    var lastnameStart = "L";
    var sql = String.Format("SELECT * FROM authors WHERE lastname LIKE '{0}%'", lastnameStart);
    var authors = _context.Authors.FromSqlRaw(sql)
        .OrderBy(a => a.LastName).TagWith("Formatted_Unsafe").ToList();
}

//FormattedRawSql_Safe();
void FormattedRawSql_Safe()
{
    var lastnameStart = "L";
    var authors = _context.Authors
        .FromSqlRaw("SELECT * FROM authors WHERE lastname LIKE '{0}%'", lastnameStart)
        .OrderBy(a => a.LastName).TagWith("Formatted_Safe").ToList();
}

//StringFromInterpolated_Unsafe();
void StringFromInterpolated_Unsafe()
{
    var lastnameStart = "L";
    string sql = $"SELECT * FROM authors WHERE lastname LIKE '{lastnameStart}%'";
    var authors = _context.Authors.FromSqlRaw(sql)
        .OrderBy(a => a.LastName).TagWith("Interpolated_Unsafe").ToList();
}

//StringFromInterpolated_StillUnsafe();
void StringFromInterpolated_StillUnsafe()
{
    var lastnameStart = "L";
    var authors = _context.Authors
        .FromSqlRaw($"SELECT * FROM authors WHERE lastname LIKE '{lastnameStart}%'")
        .OrderBy(a => a.LastName).TagWith("Interpolated_StillUnsafe").ToList();
}

//StringFromInterpolated_Safe();
void StringFromInterpolated_Safe()
{
    var lastnameStart = "L";
    var authors = _context.Authors
        .FromSqlInterpolated($"SELECT * FROM authors WHERE lastname LIKE '{lastnameStart}%'")
        .OrderBy(a => a.LastName).TagWith("Interpolated_Safe").ToList();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//this will not work, already has a cover associated with it and we aren't tracking cover here
void AddCoverToExistingBookThatHasAnUnTrackedCover()
{
    var book = _context.Books.Find(5); //The Never
    book.Cover = new Cover { DesignIdeas = "A spiral" };
    _context.SaveChanges();
}

//any dependent also needs to be in memory and tracked i.e. the book
void AddCoverToExistingBookWithTrackedCover()
{
    var book = _context.Books.Include(b => b.Cover)
        .FirstOrDefault(b => b.BookId == 5); //The Never
    book.Cover = new Cover { DesignIdeas = "A spiral" };
    _context.ChangeTracker.DetectChanges();
    // var debugview = _context.ChangeTracker.DebugView.ShortView;
}

//To be more explicitly 
void ProtectingFromUniqueFK()
{
    var TheNeverDesignIdeas = "A spirally spiral";
    var book = _context.Books.Include(b => b.Cover)
        .FirstOrDefault(b => b.BookId == 5); //The Never
    if (book.Cover != null)
    {
        book.Cover.DesignIdeas = TheNeverDesignIdeas;
    }
    else
    {
        book.Cover = new Cover { DesignIdeas = "A spirally spiral" };
    }

    _context.SaveChanges();
}

void NewBookAndCover()
{
    var book = new Book
    {
        AuthorId = 1, Title = "Call Me Ishtar", PublishDate = new DateTime(1973, 1, 1)
    };
    book.Cover = new Cover { DesignIdeas = "image of Ishtar" };
    _context.Books.Add(book);
    _context.SaveChanges();
}

void AddCoverToExistingBook()
{
    var book = _context.Books.Find(7); //Wool
    book.Cover = new Cover { DesignIdeas = "A wool scouring pad" };
    _context.SaveChanges();
}

void MultiLevelInclude()
{
    //avoid paying the price for all the change-tracking 
    var authorGraph = _context.Authors.AsNoTracking()
        .Include(a => a.Books)
        .ThenInclude(b => b.Cover)
        .ThenInclude(c => c.Artists)
        .FirstOrDefault(a => a.AuthorId == 1);

    Console.WriteLine(authorGraph?.FirstName + " " + authorGraph?.LastName);
    foreach (var book in authorGraph.Books)
    {
        Console.WriteLine("Book:" + book.Title);
        if (book.Cover != null)
        {
            Console.WriteLine("Design Ideas: " + book.Cover.DesignIdeas);
            Console.Write("Artist(s):");
            book.Cover.Artists.ForEach(a => Console.Write(a.LastName + " "));
        }
    }
}


void GetAllBooksWithTheirCovers()
{
    var booksandcovers = _context.Books.Include(b => b.Cover).ToList();
    booksandcovers.ForEach(book =>
        Console.WriteLine(
            book.Title +
            (book.Cover == null ? ": No cover yet" : ":" + book.Cover.DesignIdeas)));
}

void ReassignACover()
{
    var coverwithartist4 = _context.Covers
        .Include(c => c.Artists.Where(a => a.ArtistId == 4))
        .FirstOrDefault(c => c.CoverId == 5);

    coverwithartist4.Artists.RemoveAt(0);
    var artist3 = _context.Artists.Find(3);
    coverwithartist4.Artists.Add(artist3);
    // _context.ChangeTracker.DetectChanges();
    //need to call saveChanges here
}

void UnAssignAnArtistFromACover()
{
    var coverwithartist = _context.Covers
        .Include(c => c.Artists.Where(a => a.ArtistId == 1))
        .FirstOrDefault(c => c.CoverId == 1);
    coverwithartist.Artists.RemoveAt(0);
    // _context.Artists.Remove(coverwithartist.Artists[0]);
    _context.ChangeTracker.DetectChanges();
    // var debugview = _context.ChangeTracker.DebugView.ShortView;
    _context.SaveChanges();
}

void RetrieveAllArtistsWhoHaveCovers()
{
    var artistsWithCovers = _context.Artists.Where(a => a.Covers.Any()).ToList();
}

void RetrieveAllArtistsWithTheirCovers()
{
    var artistsWithCovers = _context.Artists.Include(a => a.Covers).ToList();

    foreach (var a in artistsWithCovers)
    {
        Console.WriteLine($"{a.FirstName} {a.LastName}, Designs to work on:");
        var primaryArtistId = a.ArtistId;
        if (a.Covers.Count() == 0)
        {
            Console.WriteLine("  No covers");
        }
        else
        {
            foreach (var c in a.Covers)
            {
                string collaborators = "";
                foreach (var ca in c.Artists.Where(ca => ca.ArtistId != primaryArtistId))
                {
                    collaborators += $"{ca.FirstName} {ca.LastName}";
                }

                if (collaborators.Length > 0)
                {
                    collaborators = $"(with {collaborators})";
                }

                Console.WriteLine($"  *{c.DesignIdeas} {collaborators}");
            }
        }
    }
}

void RetrieveACoverWithItsArtists()
{
    var coverWithArtists = _context.Covers.Include(c => c.Artists)
        .FirstOrDefault(c => c.CoverId == 1);
}

void RetrieveAnArtistWithTheirCovers()
{
    var artistWithCovers = _context.Artists.Include(a => a.Covers)
        .FirstOrDefault(a => a.ArtistId == 1);
}

void FindArtist()
{
    var artistA = _context.Artists.Find(1);
    Console.WriteLine("------------------------ " + artistA.FirstName);
}

void CreateNewCoverAndArtistTogether()
{
    var newArtist = new Artist { FirstName = "Kir", LastName = "Talmage" };
    var newCover = new Cover { DesignIdeas = "We like birds!" };
    newArtist.Covers.Add(newCover);
    _context.Artists.Add(newArtist);
    _context.SaveChanges();
}

void CreateNewCoverWithExistingArtist()
{
    var artistA = _context.Artists.Find(1);
    var cover = new Cover { DesignIdeas = "author has provide a photo" };
    cover.Artists.Add(artistA);
    _context.Covers.Add(cover);
    _context.SaveChanges();
}

void ConnectExistingArtistAndCoverObject()
{
    var artistA = _context.Artists.Find(1);
    var artistB = _context.Artists.Find(2);
    var coverA = _context.Covers.Find(1);
    coverA.Artists.Add(artistA);
    coverA.Artists.Add(artistB);
    _context.SaveChanges();
}


void CascadeDeleteInActionWhenTracked()
{
    //note : I knew that author with id 2 had books in my sample database
    var author = _context.Authors.Include(a => a.Books)
        .FirstOrDefault(a => a.AuthorId == 2);
    author.Books.Remove(author.Books[0]);
    _context.ChangeTracker.DetectChanges();
    var state = _context.ChangeTracker.DebugView.ShortView;
    //_context.SaveChanges();
}

void ModifyingRelatedDataWhenNotTracked()
{
    var author = _context.Authors.Include(a => a.Books)
        .FirstOrDefault(a => a.AuthorId == 5);
    author.Books[0].BasePrice = (decimal)12.00;

    var newContext = new PubContext();
    //newContext.Books.Update(author.Books[0]);
    newContext.Entry(author.Books[0]).State = EntityState.Modified;
    var state = newContext.ChangeTracker.DebugView.ShortView;
    newContext.SaveChanges();
}

// void ModifyingRelatedDataWhenNotTracked()
// {
//     var author = _context.Authors.Include(a => a.Books)
//         .FirstOrDefault(a => a.AuthorId == 5);
//     author.Books[0].BasePrice = (decimal)12.00;
//     
//     //creating a second context represents a new context in a disconnected application
//     var newContext=new PubContext(); 
//     //update will apply to every single object in the graph not just book[0]
//     newContext.Books.Update(author.Books[0]);
//     var state = newContext.ChangeTracker.DebugView.ShortView;
// }

void ModifyingRelatedDataWhenTracked()
{
    var author = _context.Authors.Include(a => a.Books)
        .FirstOrDefault(a => a.AuthorId == 5);
    //modify the first books base price
    author.Books[0].BasePrice = (decimal)10.00;
    // author.Books.Remove(author.Books[1]);
    //detect changes cause the ChangeTracker to update its knowledge of the state of all the objects that it's tracking
    //calling this directly since we are not using SaveChanges() which would have done this for us.
    _context.ChangeTracker.DetectChanges();
    //capture the dubug view into a variable 
    var state = _context.ChangeTracker.DebugView.ShortView;
}

void FilterUsingRelatedData()
{
    var recentAuthors = _context.Authors
        .Where(a => a.Books.Any(b => b.PublishDate.Year >= 2015))
        .ToList();
}

void LazyLoadBooksFromAnAuthor()
{
    //requires lazy loading to be set up in your app
    var author = _context.Authors.FirstOrDefault(a => a.LastName == "Howey");
    foreach (var book in author.Books)
    {
        Console.WriteLine(book.Title);
    }
}

void ExplicitLoadCollection()
{
    //retrieve an author in memory
    var author = _context.Authors.FirstOrDefault(a => a.LastName == "Howey");
    //once in memory use the context entry method, pass in object that you want to load from
    //the append the Collection method along with the Lambda for that authors books property
    //finally execute that with the load method.
    _context.Entry(author).Collection(a => a.Books).Load();
}


void Projections()
{
    var unknownTypes = _context.Authors
        .Select(a => new
        {
            AuthorId = a.AuthorId,
            Name = a.FirstName.First() + "" + a.LastName,
            Books = a.Books.Count
        })
        .ToList();
}

void ProjectionsFilter()
{
    var unknownTypes = _context.Authors
        .Select(a => new
        {
            AuthorId = a.AuthorId,
            Name = a.FirstName.First() + "" + a.LastName,
            Books = a.Books.Where(b => b.PublishDate.Year < 2000).Count()
        })
        .ToList();
}

void EagerLoadBooksWithAuthorsPublishedSince2010()
{
    var publishedDateStart = new DateTime(2010, 1, 1);
    var authors = _context.Authors.Include(a => a.Books
            .Where(b => b.PublishDate >= publishedDateStart)
            .OrderBy(b => b.Title))
        .ToList();

    authors.ForEach(a =>
    {
        Console.WriteLine($" {a.LastName} ({a.Books.Count})");
        a.Books.ForEach(b => Console.WriteLine("     " + b.Title));
    });
}

void EagerLoadBooksWithAuthors()
{
    var authors = _context.Authors.Include(a => a.Books).ToList();
    authors.ForEach(a => { Console.WriteLine($" {a.LastName} ({a.Books.Count})"); });
}

void InsertNewAuthorWithNewBook()
{
    var author = new Author { FirstName = "Lynda", LastName = "Rutledge" };
    author.Books.Add(new Book
    {
        Title = "West With Giraffes",
        PublishDate = new DateTime(2021, 2, 1)
    });
    _context.Authors.Add(author);
    _context.SaveChanges();
}

void InsertNewAuthorWith2NewBooks()
{
    var author = new Author { FirstName = "Don", LastName = "Jones" };
    author.Books.AddRange(new List<Book>
    {
        new Book { Title = "The Never", PublishDate = new DateTime(2019, 12, 1) },
        new Book { Title = "Alabaster", PublishDate = new DateTime(2019, 4, 1) }
    });
    _context.Authors.Add(author);
    _context.SaveChanges();
}

void AddNewBookToExistingAuthorInMemory()
{
    var author = _context.Authors.FirstOrDefault(a => a.LastName == "Howey");
    if (author != null)
    {
        author.Books.Add(
            new Book { Title = "Wool", PublishDate = new DateTime(2012, 1, 1) }
        );
    }

    _context.SaveChanges();
}

void AddNewBookToExistingAuthorInMemoryViaBook()
{
    var book = new Book
    {
        Title = "Shift",
        PublishDate = new DateTime(2012, 1, 1),
        AuthorId = 5
    };
    // book.Author = _context.Authors.Find(5); //known id for Hugh Howey
    _context.Books.Add(book);
    _context.SaveChanges();
}

void AddOneAuthor()
{
    var author = new Author { FirstName = "Gee", LastName = "Shek" };
    _context.Add(author);
    _context.SaveChanges();
}

void GetOzeki()
{
    var name = "Ozeki";
    var author = _context.Authors.Where(a => a.LastName == name).ToList();
}

void InsertMultipleAuthors()
{
    var newAuthors = new Author[]
    {
        new Author { FirstName = "Ruth", LastName = "Ozeki" },
        new Author { FirstName = "Sofia", LastName = "Segovia" },
        new Author { FirstName = "Ursula K.", LastName = "LeGuin" },
        new Author { FirstName = "Hugh", LastName = "Howey" },
        new Author { FirstName = "Isabelle", LastName = "Allende" }
    };
    _context.AddRange(newAuthors);
    _context.SaveChanges();
}

void InsertMultipleAuthorsPassedIn(List<Author> listOfAuthors)
{
    _context.Authors.AddRange(listOfAuthors);
    _context.SaveChanges();
}

void DeleteAnAuthor()
{
    var extraJL = _context.Authors.Find(1);
    if (extraJL != null)
    {
        _context.Authors.Remove(extraJL);
        _context.SaveChanges();
    }
}

//methods that are similar to the work flow of a web app (non tracked objects - start
void CoordinatedRetrieveAndUpdateAuthor()
{
    var author = FindThatAuthor(3);
    if (author?.FirstName == "Julie")
    {
        author.FirstName = "Julia";
        SaveThatAuthor(author);
    }
}

Author FindThatAuthor(int authorId)
{
    using var shortLivedContext = new PubContext();
    //explicitly instantiating new pub context. No tracking when no long exist due to "using"
    return shortLivedContext.Authors.Find(authorId);
}

void SaveThatAuthor(Author author)
{
    using var anotherShortLivedContext = new PubContext();
    anotherShortLivedContext.Authors.Update(author);
    anotherShortLivedContext.SaveChanges();
}
//methods that are similar to the work flow of a web app - End

void VariousOperations()
{
    var author = _context.Authors.Find(3);
    author.LastName = "Newfoundland";
    var newAuthor = new Author() { FirstName = "Dan", LastName = "Appleman" };
    _context.Authors.Add(newAuthor);
    _context.SaveChanges();
}

void RestiveAndUpdateAuthor()
{
    var author = _context.Authors.FirstOrDefault(a => a.FirstName == "Julie" && a.LastName == "Lerman");
    if (author != null)
    {
        author.FirstName = "Julia";
        _context.SaveChanges();
    }
}

void InsertAuthor()
{
    var author = new Author() { FirstName = "Frank", LastName = "Herbert" };
    _context.Authors.Add(author);
    _context.SaveChanges();
}

void FindBookName()
{
    var book1 = _context.Books.FirstOrDefault(b => b.Title.Contains("2nd Ed"));
    if (book1 != null)
    {
        Console.WriteLine($"Is this the book you were looking for => {book1.Title}");
    }
    else
    {
        Console.WriteLine("Nothing was found to match!");
    }
}

void FindIt()
{
    var authorIdTwo = _context.Authors.Find(2);
}

void QueryFilters()
{
    // var name = "Josie";
    // var author = _context.Authors.Where(s => s.FirstName == name).ToList();
    var filter = "L%";
    var authors = _context.Authors
        .Where(a => EF.Functions.Like(a.LastName, filter)).ToList();
}

void AddAuthorWithBook()
{
    var author = new Author { FirstName = "Julie", LastName = "Lerman" };
    author.Books.Add(new Book
    {
        Title = "Programming Entity Framework",
        PublishDate = new DateTime(2009, 1, 1)
    });
    author.Books.Add(new Book
    {
        Title = "Programming Entity Framework 2nd Ed",
        PublishDate = new DateTime(2010, 8, 1)
    });
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void GetAuthorsWithBooks()
{
    using var context = new PubContext();
    //Include -> for each of the authors that you find, also bring back their books
    var authors = context.Authors.Include(a => a.Books).ToList();
    foreach (var author in authors)
    {
        Console.WriteLine(author.FirstName + " " + author.LastName);
        foreach (var book in author.Books)
        {
            Console.WriteLine(book.Title);
        }
    }
}

void AddAuthor()
{
    var author = new Author { FirstName = "Josie", LastName = "Newf" };
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void GetAuthors()
{
    using var context = new PubContext();
    var authors = context.Authors.ToList();
    foreach (var author in authors)
    {
        Console.WriteLine(author.FirstName + " " + author.LastName);
    }
}

// void QueryAggregate()
// {
//     var author = _context.Authors
//         .FirstOrDefault(a => a.LastName == "Lerman");
// }

void QueryAggregate()
{
    var author = _context.Authors.OrderByDescending(a => a.FirstName)
        .FirstOrDefault(a => a.LastName == "Lerman");
}

void SortAuthors()
{
    var authorsByLastName = _context.Authors
        .OrderBy(a => a.LastName)
        .ThenBy(a => a.FirstName).ToList();
    authorsByLastName.ForEach(a => Console.WriteLine(a.LastName + "," + a.FirstName));

    var authorsDescending = _context.Authors
        .OrderByDescending(a => a.LastName)
        .ThenByDescending(a => a.FirstName).ToList();
    Console.WriteLine("**Descending Last and First**");
    authorsDescending.ForEach(a => Console.WriteLine(a.LastName + "," + a.FirstName));
    var lermans = _context.Authors.Where(a => a.LastName == "Lerman").OrderByDescending(a => a.FirstName).ToList();
}

void AddSomeMoreAuthors()
{
    _context.Authors.Add(new Author { FirstName = "Rhoda", LastName = "Lerman" });
    _context.Authors.Add(new Author { FirstName = "Don", LastName = "Jones" });
    _context.Authors.Add(new Author { FirstName = "Jim", LastName = "Christopher" });
    _context.Authors.Add(new Author { FirstName = "Stephen", LastName = "Haunts" });
    _context.SaveChanges();
}

void SkipAndTakeAuthors()
{
    var groupSize = 2;
    for (int i = 0; i < 5; i++)
    {
        var authors = _context.Authors.Skip(groupSize * i).Take(groupSize).ToList();
        Console.WriteLine($"Group {i}:");
        foreach (var author in authors)
        {
            Console.WriteLine($" {author.FirstName} {author.LastName}");
        }
    }
}